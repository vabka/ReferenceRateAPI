using System;
using System.Linq;
using ExchangeRateLoader.XML;
using FluentAssertions;
using Xunit;

namespace ExchangeRateLoader.Tests
{
    public class EnvelopeConvertingTests
    {
        [Fact]
        public void CorrectParsing()
        {
            var envelope = new Envelope
            {
                Data = new()
                {
                    Value = new()
                    {
                        new()
                        {
                            Time = "2021-01-01",
                            Values = new()
                            {
                                new()
                                {
                                    Currency = "USD",
                                    Rate = "1.0"
                                },
                                new()
                                {
                                    Currency = "JPY",
                                    Rate = "1.5"
                                }
                            }
                        },
                        new()
                        {
                            Time = "2020-01-01",
                            Values = new()
                            {
                                new()
                                {
                                    Currency = "USD",
                                    Rate = "1.1"
                                }
                            }
                        }
                    }
                }
            };
            var data = ExchangeRatesConverter.Convert(envelope).ToArray();

            data.Length.Should().Be(3);
            data[0].Date.Should().Be(new DateTimeOffset(2021, 01, 01, 0, 0, 0, TimeSpan.FromHours(0)));
            data[0].Base.Should().Be("EUR");
            data[0].Currency.Should().Be("USD");
            data[0].Rate.Should().Be(1.0m);

            data[1].Date.Should().Be(new DateTimeOffset(2021, 01, 01, 0, 0, 0, TimeSpan.FromHours(0)));
            data[1].Base.Should().Be("EUR");
            data[1].Currency.Should().Be("JPY");
            data[1].Rate.Should().Be(1.5m);

            data[2].Date.Should().Be(new DateTimeOffset(2020, 01, 01, 0, 0, 0, TimeSpan.FromHours(0)));
            data[2].Base.Should().Be("EUR");
            data[2].Currency.Should().Be("USD");
            data[2].Rate.Should().Be(1.1m);
        }
    }
}
