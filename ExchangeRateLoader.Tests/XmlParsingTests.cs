using System;
using System.IO;
using System.Xml.Serialization;
using ExchangeRateLoader.XML;
using FluentAssertions;
using Xunit;

namespace ExchangeRateLoader.Tests
{
    public class XmlParsingTests
    {
        [Fact]
        public void SampleParsedSuccessfully()
        {
            var data = File.Open("./assets/sample.xml", FileMode.Open);
            var result = EnvelopeParser.Parse(data);
            
            result.Should().NotBeNull();

            result.Subject.Should().Be("Reference rates");
            result.Sender.Name.Should().Be("European Central Bank");
            result.Data.Value.Count.Should().Be(3);
            result.Data.Value[0].Time.Should().Be("2021-03-23");
            result.Data.Value[0].Values.Count.Should().Be(2);
            result.Data.Value[0].Values[0].Currency.Should().Be("USD");
            result.Data.Value[0].Values[0].Rate.Should().Be("1.1883");
            result.Data.Value[0].Values[1].Currency.Should().Be("JPY");
            result.Data.Value[0].Values[1].Rate.Should().Be("128.99");

            result.Data.Value[1].Time.Should().Be("2021-03-22");
            result.Data.Value[1].Values.Count.Should().Be(2);
            result.Data.Value[1].Values[0].Currency.Should().Be("USD");
            result.Data.Value[1].Values[0].Rate.Should().Be("1.1926");
            result.Data.Value[1].Values[1].Currency.Should().Be("JPY");
            result.Data.Value[1].Values[1].Rate.Should().Be("129.77");

            result.Data.Value[2].Time.Should().Be("2021-03-19");
            result.Data.Value[2].Values.Count.Should().Be(2);
            result.Data.Value[2].Values[0].Currency.Should().Be("USD");
            result.Data.Value[2].Values[0].Rate.Should().Be("1.1891");
            result.Data.Value[2].Values[1].Currency.Should().Be("JPY");
            result.Data.Value[2].Values[1].Rate.Should().Be("129.54");
        }
    }
}
