using System;

namespace ExchangeAPI.Service
{
    public record ReferenceRateData(DateTimeOffset Date, string Base, string Currency, decimal Value);
}
