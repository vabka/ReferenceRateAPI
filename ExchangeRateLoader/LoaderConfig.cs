namespace ExchangeRateLoader
{
    public class LoaderConfig
    {
        public string HistoricalDataUri { get; init; } =
            "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml";

        public string FreshDataUri { get; init; } = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist-90d.xml";
    }
}