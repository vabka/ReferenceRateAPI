using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ExchangeAPI.Data.Entities
{
    [Index(nameof(Date), nameof(Currency))]
    [Index(nameof(Currency))]
    public record ExchangeRate
    {
        public DateTimeOffset Date { get; init; }
        [MaxLength(3)] public string Currency { get; init; }
        [MaxLength(3)] public string Base { get; init; }
        public decimal Rate { get; init; }
    }
}
