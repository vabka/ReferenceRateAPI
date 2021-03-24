using System;
using System.Collections.Generic;

namespace ExchangeAPI.Service
{
    public record LatestReferenceRate
    {
        public DateTimeOffset Date { get; init; }
        public string Base { get; init; }
        public IReadOnlyCollection<CurrencyRate> Rates { get; init; }
    }
}
