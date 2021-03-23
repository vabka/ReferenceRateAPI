using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ExchangeRateLoader.XML;
using ExchangeRate = ExchangeAPI.Data.Entities.ExchangeRate;

namespace ExchangeRateLoader
{
    public static class ExchangeRatesConverter
    {
        public static IEnumerable<ExchangeRate> Convert(Envelope envelope) =>
            from day in envelope.Data.Value
            let date = DateTimeOffset
                .ParseExact(day.Time, "yyyy-MM-dd", null, DateTimeStyles.AssumeUniversal)
            from exchangeRate in day.Values
            select new ExchangeRate
            {
                Date = date,
                Rate = decimal.Parse(exchangeRate.Rate),
                Currency = exchangeRate.Currency,
                Base = "EUR"
            };
    }
}
