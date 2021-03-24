using System;
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
            return Ok(new
            {
                @base,
                date = result.Date.ToString("yyyy-MM-dd"),
                rates = result.Rates.ToDictionary(x => x.Currency, x => x.Rate)
            });
        }


        [HttpGet("/{date}")]
        public async Task<ActionResult<ReferenceRateResponse>> GetAtDate([FromRoute(Name = "date")] DateTimeOffset time,
            [FromQuery(Name = "base")] string baseCurrency = "EUR",
            [FromQuery(Name = "symbols")] string? symbols = null)
        {
            var (@base, currencies) = ParseCurrencies(baseCurrency, symbols);
            var result = await _referenceRateService.GetReferenceRate(@base, currencies, time);
            if (result.Rates.Count == 0)
                return NotFound();
            return Ok(MapToResponse(result, @base));
        }

        [HttpGet("/history")]
        public async Task<ActionResult> GetHistory([FromRoute(Name = "start_at")] DateTimeOffset? start,
            [FromRoute(Name = "end_at")] DateTimeOffset? end,
            [FromQuery(Name = "base")] string baseCurrency = "EUR",
            [FromQuery(Name = "symbols")] string? symbols = null)
        {
            if (start == null)
                return BadRequest("'start_at' missing");
            if (end == null)
                return BadRequest("'end_at' missing");
            var (@base, currencies) = ParseCurrencies(baseCurrency, symbols);
            var result =
                await _referenceRateService.GetReferenceRatesHistoryAsync(@base, start.Value, end.Value, currencies);
            return Ok(new
            {
                @base,
                start_at = start.Value.ToString("yyyy-MM-dd"),
                end_at = end.Value.ToString("yyyy-MM-dd"),
                rates = result.ToDictionary(x => x.Date.ToString("yyyy-MM-dd"),
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
