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
            var serializer = new XmlSerializer(typeof(Envelope));
            var data = File.Open("./assets/sample.xml", FileMode.Open);
            var result = serializer.Deserialize(data);
            result.Should().NotBe(null).And.BeOfType<Envelope>();
            var envelope = (Envelope) result!;

            envelope.Subject.Should().Be("Reference rates");
            envelope.Sender.Name.Should().Be("European Central Bank");
            envelope.Data.Value.Count.Should().Be(3);
            envelope.Data.Value[0].Time.Should().Be("2021-03-23");
            envelope.Data.Value[0].Values.Count.Should().Be(2);
            envelope.Data.Value[0].Values[0].Currency.Should().Be("USD");
            envelope.Data.Value[0].Values[0].Rate.Should().Be("1.1883");
            envelope.Data.Value[0].Values[1].Currency.Should().Be("JPY");
            envelope.Data.Value[0].Values[1].Rate.Should().Be("128.99");

            envelope.Data.Value[1].Time.Should().Be("2021-03-22");
            envelope.Data.Value[1].Values.Count.Should().Be(2);
            envelope.Data.Value[1].Values[0].Currency.Should().Be("USD");
            envelope.Data.Value[1].Values[0].Rate.Should().Be("1.1926");
            envelope.Data.Value[1].Values[1].Currency.Should().Be("JPY");
            envelope.Data.Value[1].Values[1].Rate.Should().Be("129.77");

            envelope.Data.Value[2].Time.Should().Be("2021-03-19");
            envelope.Data.Value[2].Values.Count.Should().Be(2);
            envelope.Data.Value[2].Values[0].Currency.Should().Be("USD");
            envelope.Data.Value[2].Values[0].Rate.Should().Be("1.1891");
            envelope.Data.Value[2].Values[1].Currency.Should().Be("JPY");
            envelope.Data.Value[2].Values[1].Rate.Should().Be("129.54");
        }
    }
}
