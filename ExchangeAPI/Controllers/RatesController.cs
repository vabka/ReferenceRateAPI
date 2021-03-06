using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExchangeAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExchangeAPI.Controllers
{
    [Route("/")]
    public class RatesController : Controller
    {
        private readonly ReferenceRateService _referenceRateService;

        public RatesController(ReferenceRateService referenceRateService)
        {
            _referenceRateService = referenceRateService;
        }

        [HttpGet("/latest")]
        public async Task<ActionResult<ReferenceRateResponse>> GetLatest(
            [FromQuery(Name = "base")] string? baseCurrency = "EUR",
            [FromQuery(Name = "symbols")] string? symbols = null)
        {
            var (@base, currencies) = ParseCurrencies(baseCurrency, symbols);
            var result = await _referenceRateService.GetLatestReferenceRateAsync(@base, currencies);
            return Ok(MapToResponse(result, @base));
        }


        [HttpGet("/{date}")]
        public async Task<ActionResult<ReferenceRateResponse>> GetAtDate([FromRoute(Name = "date")] string? time,
            [FromQuery(Name = "base")] string baseCurrency = "EUR",
            [FromQuery(Name = "symbols")] string? symbols = null)
        {
            if (time == null || !DateTimeOffset.TryParseExact(time, "yyyy-MM-dd", null,
                DateTimeStyles.AssumeUniversal, out var exactDay))
                return BadRequest("'date' missing");
            var (@base, currencies) = ParseCurrencies(baseCurrency, symbols);
            var result = await _referenceRateService.GetReferenceRate(@base, currencies, exactDay);
            if (result == null)
                return NotFound();
            return Ok(MapToResponse(result, @base));
        }

        [HttpGet("/history")]
        public async Task<ActionResult> GetHistory([FromQuery(Name = "start_at")] string? start,
            [FromQuery(Name = "end_at")] string? end,
            [FromQuery(Name = "base")] string baseCurrency = "EUR",
            [FromQuery(Name = "symbols")] string? symbols = null)
        {
            if (start == null || !DateTimeOffset.TryParseExact(start, "yyyy-MM-dd", null,
                DateTimeStyles.AssumeUniversal, out var startTime))
                return BadRequest("'start_at' missing");
            if (end == null || !DateTimeOffset.TryParseExact(end, "yyyy-MM-dd", null,
                DateTimeStyles.AssumeUniversal, out var endTime))
                return BadRequest("'end_at' missing");
            var (@base, currencies) = ParseCurrencies(baseCurrency, symbols);
            var result =
                await _referenceRateService.GetReferenceRatesHistoryAsync(@base, startTime, endTime, currencies);
            return Ok(new
            {
                @base,
                start_at = start,
                end_at = end,
                rates = result.Where(x => x.Rates.Count > 0)
                    .ToDictionary(x => x.Date.ToString("yyyy-MM-dd"),
                        x => x.Rates.ToDictionary(r => r.Currency, r => r.Rate))
            });
        }

        private static ReferenceRateResponse MapToResponse(ReferenceRate referenceRate, string @base) =>
            new()
            {
                Base = @base,
                Time = referenceRate.Date.ToString("yyyy-MM-dd"),
                Rates = referenceRate.Rates.ToDictionary(x => x.Currency, x => x.Rate)
            };

        private static (string BaseCurrency, string[]? Symbols) ParseCurrencies(string? baseCurrency,
            string? symbols)
        {
            var currencies = symbols?.Split(',');
            if (currencies != null && currencies.Any(x => !IsCurrencySymbol(x)))
                throw new BadHttpRequestException("'symbols' has invalid format");
            if (baseCurrency == null || !IsCurrencySymbol(baseCurrency))
                throw new BadHttpRequestException("'base' has invalid format");

            return (baseCurrency, currencies);
        }

        private static readonly Regex SymbolRegex = new Regex(@"^[A-Z]{3}$", RegexOptions.Compiled);
        private static bool IsCurrencySymbol(string? str) => str != null && SymbolRegex.IsMatch(str);
    }
}
