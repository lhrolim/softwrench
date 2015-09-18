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
            var user = SecurityFacade.CurrentUser();
            // Chrome converts times without offsets as if they are UTC. Instead of sending the time 
            // without an offset, send the time with the users current off set so no conversion occurs.
            var userOffsetVal = 0;

            if (user.TimezoneOffset != null) {
                userOffsetVal = user.TimezoneOffset.Value;
            }
            if (userOffsetVal == 0) {
                var dt = ((DateTime)value).FromMaximoToUser(user).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK");
                writer.WriteValue(dt);
                return;
            }

            var userOffset = TimeSpan.FromMinutes(userOffsetVal * -1);
            var datetime = new DateTimeOffset(((DateTime)value).FromMaximoToUser(user), userOffset)
                .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK");
            writer.WriteValue(datetime);
        }

    }
}
