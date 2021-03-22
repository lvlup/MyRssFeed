using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Core.Extensions
{
    public static class StringExtensions
    {
        public static T DeserializeXml<T>(this string source, Encoding encoding = null) where T : class
        {
            if (string.IsNullOrEmpty(source))
                return default(T);

            if (encoding == null)
                encoding = Encoding.UTF8;

            T result = default(T);

            XmlSerializer serializer = new XmlSerializer(typeof(T), new XmlAttributeOverrides());
            try
            {
                using var ms = new MemoryStream(encoding.GetBytes(source));
                using var reader = new StreamReader(ms, encoding);
                using var xmlReader = XmlReader.Create(reader);
                result = serializer.Deserialize(xmlReader) as T;

            }
            catch (Exception)
            {
            }

            return result;
        }

        public static string SerializeToXml<T>(this T source, Encoding encoding = null)
        {
            if (source == null)
                return null;

            if (encoding == null)
                encoding = Encoding.UTF8;

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            try
            {
                StringWriter writer = new StringWriterWithEncoding(encoding);

                using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings() { Encoding = encoding }))
                {
                    serializer.Serialize(xmlWriter, source);
                }

                return writer.GetStringBuilder().ToString();
            }
            catch (Exception)
            {
            }

            return String.Empty;
        }
    }

    public sealed class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding _encoding;

        public StringWriterWithEncoding(Encoding encoding)
        {
            this._encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return _encoding; }
        }
    }
}

