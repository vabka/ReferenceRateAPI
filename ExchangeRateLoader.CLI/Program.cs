using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.Http;
using ExchangeAPI.Data;
using ExchangeRateLoader;

var rootCommand = new RootCommand
{
    new Command("init", "Initialize DB with all historical data")
    {
        Handler = CommandHandler.Create(async () =>
        {
            var loader = CreateLoader();
            await loader.InitializeDatabase();
        }),
    },
    new Command("refresh", "Add fresh data to DB")
    {
        Handler = CommandHandler.Create(async () =>
        {
            var loader = CreateLoader();
            await loader.LoadFreshData();
        })
    }
};
rootCommand.Description = "Reference exchange rate data loader";
return await rootCommand.InvokeAsync(args);

static Loader CreateLoader()
{
    var httpClient = new HttpClient();
    var dbContext = new ExchangeDataContext();
    var loader1 = new Loader(httpClient, dbContext);
    return loader1;
}
