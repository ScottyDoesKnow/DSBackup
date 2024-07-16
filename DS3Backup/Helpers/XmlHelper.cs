using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Net.ScottyDoesKnow.DsBackup.Helpers
{
    internal static class XmlHelper
    {
        public static T FromXml<T>(string xml)
        {
            using (var stringReader = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stringReader);
            }
        }

        public static string ToXml<T>(T obj)
        {
            using (var stringWriter = new StringWriter(new StringBuilder()))
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stringWriter, obj);
                return stringWriter.ToString();
            }
        }
    }
}
