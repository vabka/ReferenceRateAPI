using System;

namespace ExchangeAPI.Service
{
    public record ExchangeData(DateTimeOffset Date, string Base, string Currency, decimal Value);
}