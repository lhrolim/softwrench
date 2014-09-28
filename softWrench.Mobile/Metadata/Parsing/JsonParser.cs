using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using softWrench.Mobile.Data;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;

namespace softWrench.Mobile.Metadata.Parsing {
    internal static class JsonParser {

        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.Objects
        };


        public static ApplicationSchemaDefinition ApplicationSchemaDefinition(string json) {
            return JsonConvert.DeserializeObject<ApplicationSchemaDefinition>(json, SerializerSettings);
        }

        public static MenuDefinition ParseMenu(string json) {
            return JsonConvert.DeserializeObject<MenuDefinition>(json, SerializerSettings);
        }

        public static DataMap DataMap(JObject json) {
            return new JsonDataMapParser().FromJson(json);
        }

        public static IReadOnlyDictionary<string, string> DataOperation(string json) {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public static string ToJson(this object data) {
            return JsonConvert.SerializeObject(data, SerializerSettings);
        }

        public static T FromJson<T>(string data) {
            return JsonConvert.DeserializeObject<T>(data, SerializerSettings);
        }
    }
}
