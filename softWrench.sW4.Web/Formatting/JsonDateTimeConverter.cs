using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Formatting {
    public class JsonDateTimeConverter : IsoDateTimeConverter {

        private const string FormatWithTimeZone = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";
        private const string FormatWithoutTimeZone = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (!(value is DateTime)) {
                writer.WriteValue((string)null);
                return;
            }
            var date = ((DateTime)value);
            var user = SecurityFacade.CurrentUser();
            // Chrome converts times without offsets as if they are UTC. Instead of sending the time 
            // without an offset, send the time with the users current off set so no conversion occurs.
            var userOffsetVal = 0;

            if (user.TimezoneOffset != null) {
                userOffsetVal = user.TimezoneOffset.Value;
            }

            //https://controltechnologysolutions.atlassian.net/browse/SWWEB-1688
            var isAlreadyOnUtc = DateTimeKind.Utc.Equals(date.Kind);
            var dateConverted = isAlreadyOnUtc ? date.FromUTCToUser(user) : date.FromMaximoToUser(user);
            var formatToUse = isAlreadyOnUtc ? FormatWithoutTimeZone : FormatWithTimeZone;


            if (userOffsetVal == 0 || isAlreadyOnUtc) {
                var dt = dateConverted.ToString(formatToUse);
                writer.WriteValue(dt);
                return;
            }

            var userOffset = TimeSpan.FromMinutes(userOffsetVal * -1);
            var datetime = new DateTimeOffset(dateConverted, userOffset).ToString(FormatWithTimeZone);
            writer.WriteValue(datetime);
        }

    }
}
