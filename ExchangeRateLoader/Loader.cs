using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ExchangeAPI.Data;
using ExchangeAPI.Data.Entities;
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
            var xmlData = EnvelopeParser.Parse(xml);
            var total = xmlData.Data.Value.Count * 32 * 32;
            var data = ExchangeRatesConverter.Convert(xmlData);
            await InsertData(total, data);
        }

        public async Task LoadFreshData()
        {
            var xmlLoadingTask = LoadData(Last90DaysReferenceRatesUri);
            var lastDateLoadingTask = _dbContext.Rates
                .OrderByDescending(x => x.Date)
                .Select(x => x.Date)
                .FirstAsync();
            var xml = await xmlLoadingTask;
            var xmlData = EnvelopeParser.Parse(xml);
            var total = xmlData.Data.Value.Count * 32 * 32;
            var data = ExchangeRatesConverter.Convert(xmlData);
            var lastDate = await lastDateLoadingTask;
            await InsertData(total, data.TakeWhile(x => x.Date > lastDate));
        }

        private async Task InsertData(int total, IEnumerable<ExchangeRate> data)
        {
            Console.WriteLine($"Total lines to insert: {total}");
            var width = Console.BufferWidth;
            var processed = 0;
            var full = (double) total;
            var filled = 0;
            foreach (var page in data.Paginate(500))
            {
                _dbContext.Rates.AddRange(page);
                await _dbContext.SaveChangesAsync();
                _dbContext.ChangeTracker.Clear();
                {
                    processed += 500;
                    var tmp = (int) (processed / full * width);
                    Console.Write(new string('█', tmp - filled));
                    filled = tmp;
                }
            }
        }

        private async Task<Stream> LoadData(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
