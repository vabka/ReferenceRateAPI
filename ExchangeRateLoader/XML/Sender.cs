using System.Xml.Serialization;

namespace ExchangeRateLoader.XML
{
    [XmlRoot(ElementName = "Sender", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
    public class Sender
    {
        [XmlElement(ElementName = "name", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
        public string Name { get; init; }
    }
}
