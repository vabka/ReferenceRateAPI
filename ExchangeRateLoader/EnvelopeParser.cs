using System;
using System.IO;
using System.Xml.Serialization;
using ExchangeRateLoader.XML;

namespace ExchangeRateLoader
{
    public static class EnvelopeParser
    {
        private static readonly XmlSerializer XmlSerializer = new(typeof(Envelope));

        public static Envelope Parse(Stream xml) =>
            XmlSerializer.Deserialize(xml) as Envelope ?? throw new InvalidOperationException("Can't parse Envelope");
    }
}
