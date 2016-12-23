using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using softWrench.sW4.Data;

namespace softWrench.sW4.Web.Formatting {
    internal class DataMapXmlFormatter : MediaTypeFormatter {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        private static readonly XmlWriterSettings XmlSettings = new XmlWriterSettings { Indent = true };

        public DataMapXmlFormatter() {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));
        }

        private static void WriteToStream(object value, XmlWriter writer) {
            var json = JsonConvert.SerializeObject(value, JsonSettings);

            var xml = JsonConvert
                .DeserializeXNode($"{{\"root\":{json}}}")
                .ToString();

            writer.WriteStartElement("dataMap");

            writer.WriteRaw(xml
                .Replace("<root>", string.Empty)
                .Replace("</root>", string.Empty));

            writer.WriteEndElement();
        }

        private static void WriteToStream(object value, Stream writeStream) {
            var enumerable = value as IEnumerable<DataMap>;
            var isEnumerable = null != enumerable;

            if (false == isEnumerable) {
                enumerable = Enumerable.Repeat((DataMap)value, 1);
            }

            using (var writer = XmlWriter.Create(writeStream, XmlSettings)) {
                writer.WriteStartDocument();

                if (isEnumerable) {
                    writer.WriteStartElement("dataMaps");
                }

                foreach (var item in enumerable) {
                    WriteToStream(item, writer);
                }

                if (isEnumerable) {
                    writer.WriteEndElement();
                }

                writer.WriteEndDocument();
            }

            writeStream.Flush();
        }

        public override bool CanReadType(Type type) {
            return false;
        }

        public override bool CanWriteType(Type type) {
            if (type == typeof(DataMap)) {
                return true;
            }

            var enumerableType = typeof(IEnumerable<DataMap>);
            return enumerableType.IsAssignableFrom(type);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext) {
            var task = Task
                .Factory
                .StartNew(() => WriteToStream(value, writeStream));
            return task;
        }
    }
}