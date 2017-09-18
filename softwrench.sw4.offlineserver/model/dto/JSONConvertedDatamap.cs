using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using softWrench.sW4.Data;
using softWrench.sW4.Web.Formatting;

namespace softwrench.sw4.offlineserver.model.dto {
    public class JSONConvertedDatamap : DataMap {


        public static JSONConvertedDatamap FromFieldsAndMappingType([NotNull] string application, [NotNull] IDictionary<string, object> fields, Type mappingType = null, bool rowstampsHandled = false) {
            return new JSONConvertedDatamap(new DataMap(application, fields, mappingType, rowstampsHandled));
        }

        public static JSONConvertedDatamap FromFields([NotNull] string application, [NotNull] IDictionary<string, object> fields, string idFieldName) {
            return new JSONConvertedDatamap(new DataMap(application, fields, idFieldName));
        }

        public JSONConvertedDatamap() {

        }

        public string JsonFields {
            get { return GetStringAttribute("JSONFields"); }
            set { SetAttribute("JSONFields", value); }
        }

        [JsonIgnore]
        public DataMap OriginalDatamap { get; set; }

        public JSONConvertedDatamap(DataMap datamap, bool rowstampsHandled = false) : base(datamap.Application, datamap.Fields, null, rowstampsHandled) {
            var st = JsonConvert.SerializeObject(this, Formatting.None,
             new JsonSerializerSettings {
                 ContractResolver = new CamelCasePropertyNamesContractResolver(),
                 Converters = new List<JsonConverter>() { new JsonDateTimeConverter() }

             });
            Clear();
            this["JSONFields"] = st;
            this["Application"] = datamap.Application;
            this["Id"] = datamap.Id;
            this["Approwstamp"] = datamap.Approwstamp;

            Approwstamp = datamap.Approwstamp;
            Id = datamap.Id;
            Application = datamap.Application;

            OriginalDatamap = datamap;
        }

    }
}
