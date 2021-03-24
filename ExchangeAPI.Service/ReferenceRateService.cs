using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExchangeAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ExchangeAPI.Service
{
    public class ReferenceRateService
    {
        private readonly ReferenceRatesDbContext _dbContext;

        public ReferenceRateService(ReferenceRatesDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ReferenceRate> GetLatestReferenceRateAsync(
            string @base,
            string[]? currencies)
            => await GetReferenceRate(@base, currencies, await GetLatestReferenceRateDate());

        public async Task<ReferenceRate> GetReferenceRate(string @base, string[]? currencies,
            DateTimeOffset time)
        {
            var data = await _dbContext.Rates
                .AsNoTracking()
                .Where(x => x.Date == time &&
                            (currencies == null || currencies.Contains(x.Currency) || x.Currency == @base))
                .Select(x => new CurrencyRate(x.Currency, x.Rate))
                .ToArrayAsync();
            return new ReferenceRate
            {
                Date = time,
                Rates = ConvertWhenNecessary(@base, data).ToArray()
            };
        }

        private async Task<DateTimeOffset> GetLatestReferenceRateDate() =>
            await _dbContext.Rates
                .AsNoTracking()
                .OrderByDescending(x => x.Date)
                .Select(x => x.Date)
                .FirstAsync();

        public async Task<ReferenceRate[]> GetReferenceRatesHistoryAsync(string @base,
            DateTimeOffset start,
            DateTimeOffset end,
            string[]? currencies = null)
        {
            var data = await _dbContext.Rates
                .Where(x => x.Date >= start && x.Date <= end &&
                            (currencies == null || currencies.Contains(x.Currency) || x.Currency == @base)).Select(x =>
                    new
                    {
                        x.Currency,
                        x.Date,
                        x.Rate
                    })
                .ToArrayAsync();
            return data.GroupBy(x => x.Date)
                .Select(x => new ReferenceRate
                {
                    Date = x.Key,
                    Rates = ConvertWhenNecessary(@base, x.Select(e => new CurrencyRate(e.Currency, e.Rate)))
                })
                .ToArray();
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
