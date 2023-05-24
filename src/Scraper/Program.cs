using System;
using System.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Scraper.Services;
using Shared;

var config = new ConfigurationManager()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

DbHandler.Options dbOptions;
if (args.Length > 0)
    if (Path.Exists(args[0]))
        dbOptions = new DbHandler.Options { BasePath = args[0] };
    else throw new Exception("Path does not exist");
else dbOptions = config.GetSection("DbHandler").Get<DbHandler.Options>()
                 ?? throw new Exception("No DbHandler options found");

//create services
var services = new ServiceCollection();
services.AddSingleton(dbOptions);
services.AddSingleton<DbHandler>();
services.AddLogging(builder => builder.AddConsole().AddDebug());
services.AddHttpClient();
services.AddSingleton<DataProvider>();
services.Configure<DishDashOptions>(config.GetSection("Mensa"));

var provider = services.BuildServiceProvider();
var dataProvider = provider.GetRequiredService<DataProvider>();
await dataProvider.CollectDataAndStore(CancellationToken.None);

await provider.DisposeAsync();
