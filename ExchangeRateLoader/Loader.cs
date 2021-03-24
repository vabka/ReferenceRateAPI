using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExchangeAPI.Data;
using ExchangeAPI.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExchangeRateLoader
{
    public class Loader
    {
        private readonly HttpClient _httpClient;
        private readonly ReferenceRatesDbContext _dbContext;
        private readonly bool _consoleMode;
        private readonly ILogger<Loader> _logger;
        private readonly LoaderConfig _config;

        public Loader(HttpClient httpClient, 
            ReferenceRatesDbContext dbContext,
            ILogger<Loader> logger,
            LoaderConfig config,
            bool consoleMode = false)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _logger = logger;
            _config = config;
            _consoleMode = consoleMode;
        }

        public async Task InitializeDatabase(CancellationToken cancellationToken = default)
        {
            await using var xml = await LoadData(_config.HistoricalDataUri);
            var xmlData = EnvelopeParser.Parse(xml);
            var total = xmlData.Data.Value.Count * 32;
            var data = ExchangeRatesConverter.Convert(xmlData);
            await InsertData(total, data, cancellationToken);
        }

        public async Task LoadFreshData(CancellationToken cancellationToken = default)
        {
            var xmlLoadingTask = LoadData(_config.FreshDataUri);
            var lastDateLoadingTask = _dbContext.Rates
                .OrderByDescending(x => x.Date)
                .Select(x => x.Date)
                .FirstAsync(cancellationToken);
            var xml = await xmlLoadingTask;
            var xmlData = EnvelopeParser.Parse(xml);
            const int numberOfCurrencies = 32;
            var total = xmlData.Data.Value.Count * numberOfCurrencies;
            var data = ExchangeRatesConverter.Convert(xmlData);
            var lastDate = await lastDateLoadingTask;
            await InsertData(total, data.TakeWhile(x => x.Date > lastDate), cancellationToken);
        }

        private async Task InsertData(int total, IEnumerable<ExchangeRate> data, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Saving ~{Count} reference rates", total);

            var width = Console.BufferWidth;
            var processed = 0;
            var full = (double) total;
            var filled = 0;
            foreach (var page in data.Paginate(500))
            {
                _dbContext.Rates.AddRange(page);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();

                if (_consoleMode)
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
            _logger.LogInformation("Loading data from {Uri}", uri);
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
