using System.Xml.Serialization;

namespace ExchangeRateLoader.XML
{
    [XmlRoot(ElementName = "Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    public class ExchangeRate
    {
        [XmlAttribute(AttributeName = "currency")]
        public string Currency { get; init; }

        [XmlAttribute(AttributeName = "rate")] public string Rate { get; init; }
    }
}
