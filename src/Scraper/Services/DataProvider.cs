using System.Globalization;
using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;

namespace Scraper.Services;

public class DataProvider
{
    private readonly IHttpClientFactory _factory;
    private readonly IOptions<DishDashOptions> _options;
    private readonly ILogger<DataProvider> _logger;
    private readonly DbHandler _dbHandler;
    private NumberFormatInfo? _infCache;

    public DataProvider(IHttpClientFactory factory, IOptions<DishDashOptions> options, ILogger<DataProvider> logger, DbHandler dbHandler)
        => (_factory, _options, _logger, _dbHandler) = (factory, options, logger, dbHandler);

    public async Task CollectDataAndStore(CancellationToken cancellationToken)
    {
        var config = _options.Value;
        var baseUrl = config.BaseUrl;
        var client = _factory.CreateClient(baseUrl);

        client.BaseAddress = new Uri(baseUrl);

        foreach (var side in config.Sides)
        {
            try
            {
                var result = await client.GetStringAsync(side, cancellationToken);

                var Count = 0;
                await foreach (var record in ProcessResult(result, side, cancellationToken))
                {
                    Count++;
                }

                _logger.LogInformation("Processed Side {side} got {count} Days", side, Count);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to process side {side}", side);
            }
        }
    }

    private const string SectionEnd = "</section>";
    private const string SectionBegin = """<section class="fmc-day""";
    private const string PageTitle = "page-title";
    private const string H4ZusatzstoffeH4 = "<h4>Zusatzstoffe</h4>";

    private async IAsyncEnumerable<Day> ProcessResult(string result, string side, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var start = result.IndexOf(PageTitle, StringComparison.InvariantCultureIgnoreCase);
        var end = result.IndexOf(H4ZusatzstoffeH4, StringComparison.InvariantCultureIgnoreCase);
        var cut = result.Substring(start, end - start);
        var name = cut.Substring(PageTitle.Length + 2, cut.IndexOf("</", StringComparison.Ordinal) - 12);

        var count = 0;
        end = 0;
        while (true)
        {
            start = cut.IndexOf(SectionBegin, end, StringComparison.InvariantCultureIgnoreCase);
            if (start == -1) break;
            end = cut.IndexOf(SectionEnd, start, StringComparison.InvariantCultureIgnoreCase);
            if (end == -1) break;
            yield return await ProcessSection(cut.Substring(start, end - start + SectionEnd.Length), side, name, cancellationToken);
            if (count++ > 10) throw new Exception("Too many sections");
        }
    }

    private async Task<Day> ProcessSection(string section, string side, string name, CancellationToken cancellationToken)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(section);
        var c = doc.DocumentNode.FirstChild;
        var interest = c.ChildNodes.Where(x => x.Name == "div").ToArray();
        var date = interest.FirstOrDefault()?.InnerText?.Trim();
        if (date is null) throw new ArgumentNullException(nameof(section), "Has No date");
        var nodes = interest.Skip(1).FirstOrDefault()?.ChildNodes.FindFirst("ul").ChildNodes;
        if (nodes is null) throw new ArgumentNullException(nameof(section), "Has No nodes");
        var daySplit = date.Split('\t', StringSplitOptions.RemoveEmptyEntries);
        if (daySplit.Length < 2) throw new ArgumentException("Has No day", nameof(section));
        var record = await _dbHandler.GetOrCreateDayRecord(Enum.Parse<DayOfWeak>(daySplit[0].Trim()),
            DateOnly.ParseExact(daySplit[1].Trim(), /*13.03.2023*/"dd.MM.yyyy", CultureInfo.InvariantCulture),
            side, name, cancellationToken);
        foreach (var node in nodes.Where(x => x.Name == "li"))
        {
            var result = await ProcessNode(node, cancellationToken);
            record.Meals.Add(result.Hash);
        }
        return record;
    }

    private async Task<Meal> ProcessNode(HtmlNode node, CancellationToken cancellationToken)
    {
        var data = node.ChildNodes.FindFirst("div");
        var cat = node.Attributes.FirstOrDefault(x => x.Name == "data-cat")?.Value;
        var location = node.FirstOrDefaultByClass("fmc-item-location")?.InnerText;
        var icons = node.FirstOrDefaultByClass("fmc-item-icons")?.ChildNodes?.FindFirst("span")?.ChildNodes?.FindFirst("svg")?.OuterHtml;
        var line = node.ChildNodes.FindFirst("div");
        var title = line.FirstOrDefaultByClass("fmc-item-title")?.InnerText;
        var price = line.FirstOrDefaultByClass("fmc-item-price")?.InnerText;

        decimal priceFormat;
        try
        {
            priceFormat = decimal.Parse(price ?? "-1", NumberStyles.Currency, _infCache ??= new NumberFormatInfo {
                CurrencySymbol = "\u20AC",
                CurrencyDecimalSeparator = ","
            });
        }
        catch (Exception)
        {
            priceFormat = decimal.Parse(price ?? "-1");
        }
        if (priceFormat >= 80) //bc 0.80€ is the lowest price know. 80€ is a bit high for a meal probably the format is wrong
        {
            _logger.LogWarning("Price {price} is too high fixing (/100)", priceFormat);
            priceFormat /= 100;
        }

        var icon = _dbHandler.GetOrCreateImage(icons ?? "", cancellationToken);
        return await _dbHandler.GetOrCreateMeal((MealCategory)byte.Parse(cat ?? "0"), title?.Trim() ?? "-", priceFormat, location?.Trim() ?? "-", await icon, cancellationToken);
    }
}
