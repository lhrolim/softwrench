using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using softWrench.Mobile.Data;

namespace softWrench.Mobile.Metadata.Parsing
{
    internal class JsonDataMapParser
    {
        private static DataMap ParseDataMap(JObject json)
        {
            var application = json.Value<string>("application");
            var fields = new Dictionary<string, string>();

            foreach (JProperty fieldJson in json["fields"])
            {
                var value = fieldJson
                    .Value
                    .Value<string>();

                fields[fieldJson.Name] = value;
            }

            return new DataMap(application, fields);
        }

        public DataMap FromJson(JObject json)
        {
            return ParseDataMap(json);
        }
    }
}