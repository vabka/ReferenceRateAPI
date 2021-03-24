using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExchangeAPI.Controllers
{
    public class ReferenceRateResponse
    {
        [JsonPropertyName("base")] public string Base { get; init; }
        [JsonPropertyName("date")] public string Time { get; init; }
        [JsonPropertyName("rates")] public Dictionary<string, decimal> Rates { get; init; }
    }
}