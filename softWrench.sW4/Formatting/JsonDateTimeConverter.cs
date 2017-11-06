using System;
using System.Globalization;
using cts.commons.portable.Util;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Formatting {
    public class JsonDateTimeConverter : IsoDateTimeConverter {

        private const string FormatWithoutTimeZone = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";

        private static readonly ILog Log = LogManager.GetLogger(typeof(JsonDateTimeConverter));

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            try {
                DoWriteJson(writer, value);
            } catch (Exception e) {
                Log.Error(e);
                throw;
            }

        }

        private static void DoWriteJson(JsonWriter writer, object value) {
            var user = SecurityFacade.CurrentUser();
            // Chrome converts times without offsets as if they are UTC. Instead of sending the time 
            // without an offset, send the time with the users current off set so no conversion occurs.
            var userOffsetVal = 0;

            if (value is DateTimeOffset) {
                var dto = (DateTimeOffset)value;
                if (user.TimezoneOffset != null) {
                    userOffsetVal = user.TimezoneOffset.Value;
                }
                var dateTimeOffset = new DateTimeOffset(dto.DateTime, TimeSpan.FromMinutes(userOffsetVal * -1));
                writer.WriteValue(dateTimeOffset.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (!(value is DateTime)) {
                writer.WriteValue((string)null);
                return;
            }
            var date = ((DateTime)value);
           
            

            if (user.TimezoneOffset != null) {
                userOffsetVal = user.TimezoneOffset.Value;
            }

            //https://controltechnologysolutions.atlassian.net/browse/SWWEB-1688
            var isAlreadyOnUtc = DateTimeKind.Utc.Equals(date.Kind);
            var isAlreadyLocal = DateTimeKind.Local.Equals(date.Kind);


            DateTime dateConverted;

            if (isAlreadyLocal) {
                dateConverted = date.FromServerToUser(user);
            } else {
                dateConverted = isAlreadyOnUtc ? date.FromUTCToUser(user) : date.FromMaximoToUser(user);
            }



            //let´s double check if UTC, cause we can´t afford to conver it then
            isAlreadyOnUtc = DateTimeKind.Utc.Equals(dateConverted.Kind);
            var isLocal = DateTimeKind.Local.Equals(dateConverted.Kind);

            if (userOffsetVal == 0 || isAlreadyOnUtc || isLocal) {
                var dt = dateConverted.ToString(FormatWithoutTimeZone);

                writer.WriteValue(dt);
                return;
            }

            var userOffset = TimeSpan.FromMinutes(userOffsetVal * -1);
            Log.DebugFormat("converting datetime {0} of kind {1} using offset {2} ".Fmt(dateConverted, dateConverted.Kind, userOffset));
            var datetime = new DateTimeOffset(dateConverted, userOffset).ToString(FormatWithoutTimeZone);
            writer.WriteValue(datetime);
        }
    }
}
