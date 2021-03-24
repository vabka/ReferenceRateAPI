using System;
using System.Collections.Generic;

namespace ExchangeAPI.Service
{
    public record ReferenceRate
    {
        public DateTimeOffset Date { get; init; }
        public IReadOnlyCollection<CurrencyRate> Rates { get; init; } = null!;
    }
}
