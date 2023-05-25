using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Shared;

public class DbHandler : IAsyncDisposable
{
    public class Options
    {
        public required string BasePath { get; set; }
    }
    private readonly string _basePath;
    private readonly ILogger<DbHandler> _logger;
    private List<Meal> _meals;
    private List<Image> _images;
    private List<Day> _days;
    private bool _hasChanges = false;
    private bool _isLoaded = false;

    public DbHandler(Options opts, ILogger<DbHandler> logger)
    {
        _basePath = opts.BasePath;
        _logger = logger;
        _meals = null!;
        _images = null!;
        _days = null!;
    }

    public async Task LoadIfNotLoaded()
    {
        if (_isLoaded) return;
        await using var mealsStream = File.OpenRead(Path.Combine(_basePath, "meals.json"));
        _meals = await JsonSerializer.DeserializeAsync<List<Meal>>(mealsStream) ?? new List<Meal>();

        await using var imagesStream = File.OpenRead(Path.Combine(_basePath, "images.json"));
        _images = await JsonSerializer.DeserializeAsync<List<Image>>(imagesStream) ?? new List<Image>();

        await using var daysStream = File.OpenRead(Path.Combine(_basePath, "days.json"));
        _days = await JsonSerializer.DeserializeAsync<List<Day>>(daysStream) ?? new List<Day>();
        _isLoaded = true;
        if (!CheckDataIntegrity())
        {
            _logger.LogCritical("Data integrity check failed");
        }
        PrintStats();
    }

    private void PrintStats()
    {
        _logger.LogInformation("Loaded {Meals} Meals, {Images} Images, {Days} Days", _meals.Count, _images.Count, _days.Count);
    }

    public async Task SaveIfChanged()
    {
        await LoadIfNotLoaded();
        if (!_hasChanges)
        {
            _logger.LogInformation("No changes to save");
            return;
        }

        _logger.LogInformation("Saving changes to disk");

        var options = new JsonSerializerOptions {
            WriteIndented = true,
        };

        await using var mealsStream = File.Create(Path.Combine(_basePath, "meals.json"));
        await JsonSerializer.SerializeAsync(mealsStream, _meals, options);

        await using var imagesStream = File.Create(Path.Combine(_basePath, "images.json"));
        await JsonSerializer.SerializeAsync(imagesStream, _images, options);

        await using var daysStream = File.Create(Path.Combine(_basePath, "days.json"));
        await JsonSerializer.SerializeAsync(daysStream, _days, options);

        _hasChanges = false;
        PrintStats();
    }

    private bool CheckDataIntegrity()
    {
        foreach (var m in _meals)
        {
            var mealImageIntegrity = _images.Any(x => x.Hash == m.Image);

            if (!mealImageIntegrity)
            {
                _logger.LogError("Meal image integrity check failed! Image Not Found: {Image}", m.Image);
            }
            if (m.Hash != HashMeal(m))
            {
                _logger.LogError("Meal hash integrity check failed! Hash is invalid: {Hash} -> {Should}\n{M}", m.Hash, HashMeal(m), JsonSerializer.Serialize(m));
            }
        }
        foreach (var d in _days)
        {
            foreach (var m in d.Meals)
            {
                var foundHash = _meals.Any(meal => meal.Hash == m);
                if (!foundHash)
                {
                    _logger.LogError("Day meal integrity check failed! Meal Not Found: {Meal}", m);
                }
            }
        }
        foreach (var i in _images)
        {
            if (i.Hash != Hash(i.Svg))
            {
                _logger.LogError("Image hash integrity check failed! Hash is invalid: {Hash} -> {Should}", i.Hash, Hash(i.Svg));
            }
        }

        return true;
    }

    public async Task<Day> GetOrCreateDayRecord(DayOfWeak day, DateOnly date, string src, string srcName, CancellationToken cancellationToken)
    {
        await LoadIfNotLoaded();

        var dayRecord = _days.FirstOrDefault(d => d.Date == date && d.DayOfWeak == day && d.Source == src && d.SourceName == srcName);
        if (dayRecord is not null) return dayRecord;
        dayRecord = new Day {
            Date = date,
            DayOfWeak = day,
            Source = src,
            SourceName = srcName,
            Meals = new List<Guid>()
        };
        _days.Add(dayRecord);
        _hasChanges = true;
        return dayRecord;
    }

    public async Task<Image> GetOrCreateImage(string icons, CancellationToken cancellationToken)
    {
        await LoadIfNotLoaded();

        var hash = Hash(icons);
        var icon = _images.FirstOrDefault(i => i.Hash == hash);
        if (icon is not null) return icon;
        icon = new Image {
            Svg = icons,
            Hash = hash
        };
        _images.Add(icon);
        _hasChanges = true;
        return icon;
    }

    private static Guid Hash(string value)
    {
        return new Guid(MD5.HashData(Encoding.UTF8.GetBytes(value)));
    }

    private static Guid HashMeal(Meal meal)
    {
        return Hash(JsonSerializer.Serialize(new {
            meal.AllergensAndAdditives,
            meal.Category,
            meal.Image,
            meal.Location,
            meal.Name,
            meal.Price,
        }));
    }

    public async Task<Meal> GetOrCreateMeal(MealCategory category, string title, decimal price, string location, Image image, CancellationToken cancellationToken)
    {
        await LoadIfNotLoaded();
        var (name, aaa) = ExtractAllergensAndAdditives(title);

        var hash = Hash(JsonSerializer.Serialize(new {
            AllergensAndAdditives = aaa,
            Category = category,
            Image = image.Hash,
            Location = location,
            Name = name,
            Price = price,
        }));

        var meal = _meals.FirstOrDefault(m => m.Hash == hash);
        if (meal is not null) return meal;
        meal = new Meal {
            Category = category,
            Name = name,
            AllergensAndAdditives = aaa,
            Price = price,
            Location = location,
            Image = image.Hash,
            Hash = hash,
        };

        _meals.Add(meal);
        _hasChanges = true;
        return meal;
    }

    private (string name, AllergensAndAdditives aaa) ExtractAllergensAndAdditives(string title)
    {
        //format: Schnitzel paniert mit Pfefferrahmsauce (A,C,I,G,I,L,3,5)
        var flag = AllergensAndAdditives.None;
        var startIndex = title.LastIndexOf('(') + 1;
        var endIndex = title.LastIndexOf(')');
        if (startIndex == -1 || endIndex == -1)
        {
            _logger.LogDebug("No allergens or additives found in '{Title}'", title);
            return (title, AllergensAndAdditives.None);
        }
        var extras = title[startIndex..endIndex];
        if (extras.Contains("pro"))
        {
            _logger.LogDebug("No allergens or additives found in '{Title}', found a Quantity instead '{Extras}'", title, extras);
            return (title, AllergensAndAdditives.None);
        }
        foreach (var allergenOrAdditive in extras.Split(','))
        {
            if (byte.TryParse(allergenOrAdditive, out var val) && val is <= 10 and > 0)
                flag |= (AllergensAndAdditives)Enum.Parse<AllergensAndAdditivesIdentifier>('_' + allergenOrAdditive);
            else if (Enum.TryParse<AllergensAndAdditivesIdentifier>(allergenOrAdditive, out var res))
                flag |= (AllergensAndAdditives)res;
            else
                _logger.LogWarning("Unknown Allergen or Additive '{AllergenOrAdditive}' on '{Title}'", allergenOrAdditive, title);
        }
        return (title[..(startIndex - 1)].Trim(), flag);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await SaveIfChanged();
        GC.SuppressFinalize(this);
    }
}
