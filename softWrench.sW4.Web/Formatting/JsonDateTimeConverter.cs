using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Formatting {
    public class JsonDateTimeConverter : IsoDateTimeConverter {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var datetime = String.Empty;
            if (value is DateTime) {
                var user = SecurityFacade.CurrentUser();
                datetime = ((DateTime)value).FromMaximoToUser(user).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFKzzz");
            }
            writer.WriteValue(datetime);
        }

    }
}
