using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Formatting {
    public class JsonDateTimeConverter : IsoDateTimeConverter {

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (!(value is DateTime)) {
                writer.WriteValue((string)null);
                return;
            }
            string datetime;
            var user = SecurityFacade.CurrentUser();
            // Chrome converts times without offsets as if they are UTC. Instead of sending the time 
            // without an offset, send the time with the users current off set so no conversion occurs.
            var userOffsetVal = 0;
            var dateConverted = ((DateTime)value).FromMaximoToUser(user);
            if (userOffsetVal == 0) {
                datetime = dateConverted.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK");
            } else {
                if (user.TimezoneOffset != null) {
                    userOffsetVal = user.TimezoneOffset.Value;
                }
                var userOffset = TimeSpan.FromMinutes(userOffsetVal * -1);
                datetime =
                    new DateTimeOffset(dateConverted, userOffset).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK");
            }
            writer.WriteValue(datetime);
        }

    }
}
