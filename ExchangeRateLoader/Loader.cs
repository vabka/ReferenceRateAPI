using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ExchangeAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ExchangeRateLoader
{
    public class Loader
    {
        const string HistoricalReferenceRatesUri = "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist.xml";

        private const string Last90DaysReferenceRatesUri =
            "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-hist-90d.xml";

        private readonly HttpClient _httpClient;
        private readonly ExchangeDataContext _dbContext;

        public Loader(HttpClient httpClient, ExchangeDataContext dbContext)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
        }

        public async Task InitializeDatabase()
        {
            await using var xml = await LoadData(HistoricalReferenceRatesUri);
            var data = ExchangeRatesConverter.Convert(EnvelopeParser.Parse(xml));
            await InsertData(data);
        }
        
        public async Task LoadFreshData()
        {
            var xmlLoadingTask = LoadData(Last90DaysReferenceRatesUri);
            var lastDateLoadingTask = _dbContext.Rates
                .OrderByDescending(x => x.Date)
                .Select(x => x.Date)
                .FirstAsync();
            var xml = await xmlLoadingTask;
            var data = ExchangeRatesConverter.Convert(EnvelopeParser.Parse(xml));
            var lastDate = await lastDateLoadingTask;
            await InsertData(data.TakeWhile(x => x.Date > lastDate));
        }

        private async Task InsertData(IEnumerable<ExchangeAPI.Data.Entities.ExchangeRate> data)
        {
            _dbContext.Rates.AddRange(data);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<Stream> LoadData(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
