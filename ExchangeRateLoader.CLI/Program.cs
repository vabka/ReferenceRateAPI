using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.Http;
using ExchangeAPI.Data;
using ExchangeRateLoader;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var rootCommand = new RootCommand();
var initCommand = new Command("init", "Initialize DB with all historical data")
{
    Handler = CommandHandler.Create<DatabaseType, string>(async (dbType, connectionString) =>
    {
        var loader = CreateLoader(dbType, connectionString);
        await loader.InitializeDatabase();
    }),
};
initCommand.AddOption(new Option<DatabaseType>("--db-type", () => DatabaseType.Sqlite, "Database Type"));
initCommand.AddOption(new Option<string>("--connection-string", () => @"Data Source=E:\\exchange.db", "Connection string"));
rootCommand.Description = "Reference exchange rate data loader";
rootCommand.Add(initCommand);
return await rootCommand.InvokeAsync(args);

static Loader CreateLoader(DatabaseType dbType, string connectionString)
{
    var loggerFactory = LoggerFactory.Create(l => l.SetMinimumLevel(LogLevel.Information));
    var httpClient = new HttpClient();
    var dbContext = dbType switch
    {
        DatabaseType.SqlServer => new ReferenceRatesDbContext(new DbContextOptionsBuilder<ReferenceRatesDbContext>()
            .UseSqlServer(connectionString).Options),
        DatabaseType.Sqlite => new ReferenceRatesDbContext(new DbContextOptionsBuilder<ReferenceRatesDbContext>()
            .UseSqlite(connectionString).Options),
        _ => throw new ArgumentOutOfRangeException(nameof(dbType), dbType, null)
    };
    var loader1 = new Loader(httpClient, dbContext, loggerFactory.CreateLogger<Loader>(), new LoaderConfig(), true);
    return loader1;
}
