using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ExchangeRateLoader.XML
{
    [XmlRoot(ElementName = "Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    public class ExchangeRates
    {
        [XmlAttribute(AttributeName = "time")] public string Time { get; init; }
        [XmlElement(ElementName = "Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
        public List<ExchangeRate> Values { get; init; }
    }
}
