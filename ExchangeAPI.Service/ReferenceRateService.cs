using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExchangeAPI.Data;
using ExchangeAPI.Service;
using Microsoft.EntityFrameworkCore;

namespace ExchangeAPI.Service
{
    public class ReferenceRateService
    {
        private readonly ExchangeDataContext _dbContext;

        public ReferenceRateService(ExchangeDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<LatestReferenceRate> GetLatestExchangeRatesAsync(string baseCurrency,
            IReadOnlySet<string>? foreignCurrencies = null)
        {
            var lastDate = await _dbContext.Rates
                .AsNoTracking()
                .OrderByDescending(x => x.Date)
                .Select(x => x.Date)
                .FirstAsync();
            var data = await _dbContext.Rates
                .AsNoTracking()
                .Where(x => x.Date == lastDate &&
                            (foreignCurrencies != null &&
                             foreignCurrencies.Contains(x.Currency) ||
                             x.Currency == baseCurrency))
                .Select(x => new CurrencyRate(x.Currency, x.Rate))
                .ToArrayAsync();
            return new LatestReferenceRate
            {
                Base = baseCurrency,
                Date = lastDate,
                Rates = data
            };
        }

        public async Task<HistoricalExchangeRate> GetHistoricalExchangeRatesAsync(string baseCurrency,
            DateTimeOffset start,
            DateTimeOffset end,
            string[]? foreignCurrencies = null)
        {
            var query = _dbContext.Rates
                .Where(x => x.Date >= start && x.Date <= end &&
                            (foreignCurrencies != null &&
                             foreignCurrencies.Contains(x.Currency) ||
                             x.Currency == baseCurrency));
            var data = await query.Select(x => new
                {
                    x.Currency,
                    x.Date,
                    x.Rate
                })
                .ToArrayAsync();
            return new HistoricalExchangeRate
            {
                Base = baseCurrency,
                Start = start,
                End = end,
                Rates = data.GroupBy(x => x.Date)
                    .Select(x => new ExchangeRatesAtDate
                    {
                        Date = x.Key,
                        Rates = ConvertWhenNecessary(baseCurrency, x.Select(e => new CurrencyRate(e.Currency, e.Rate)))
                    })
                    .ToArray()
            };
        }

        private static CurrencyRate[] ConvertWhenNecessary(string baseCurrency, IEnumerable<CurrencyRate> rates)
        {
            var tmp = rates.ToArray();
            return baseCurrency != "EUR"
                ? Convert(tmp, baseCurrency).ToArray()
                : tmp;
        }

        public static IEnumerable<CurrencyRate> Convert(CurrencyRate[] rates, string baseCurrency)
        {
            var baseRate = rates.First(rate => rate.Currency == baseCurrency);
            var euros = 1m / baseRate.Rate;
            foreach (var currencyRate in rates.Where(rate => rate.Currency != baseRate.Currency))
                yield return new CurrencyRate(currencyRate.Currency, currencyRate.Rate * euros);
        }
    }
}

public record HistoricalExchangeRate
{
    public string Base { get; init; }
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset End { get; init; }
    public ExchangeRatesAtDate[] Rates { get; init; }
}

public struct ExchangeRatesAtDate
{
    public DateTimeOffset Date { get; init; }
    public CurrencyRate[] Rates { get; init; }
}
