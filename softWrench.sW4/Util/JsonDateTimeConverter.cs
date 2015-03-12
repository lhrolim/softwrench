using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using softWrench.sW4.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Util {
    public class JsonDateTimeConverter : IsoDateTimeConverter {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var datetime = String.Empty;
            if (value is DateTime) {
                var user = SecurityFacade.CurrentUser();
                datetime = ((DateTime)value).FromMaximoToUser(user).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFKzzz");
            }
            writer.WriteValue(datetime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
