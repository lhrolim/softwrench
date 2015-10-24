using System;
using cts.commons.portable.Util;
using log4net;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Util {
    public class ConversionUtil {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ConversionUtil));

        public static object ConvertFromMetadataType(string type, string stValue) {
            //TODO: review.

            if (type == "varchar" || type == "string") {
                return stValue;
            }
            if (type.EqualsAny("smallint","int","integer")) {
                if (stValue.EqualsAny("True", "False")) {
                    return Convert.ToBoolean(stValue);
                }
                return Convert.ToInt32(stValue);
            }
            if (type == "bigint") {
                return Convert.ToInt64(stValue);
            }
            if (type == "datetime" || type == "timestamp") {

                return HandleDateConversion(stValue);
            }
            if (type == "decimal") {
                return Convert.ToDecimal(stValue);
            }
            if (type == "float" || type == "double") {
                return Convert.ToDouble(stValue);
            }
            if (type == "boolean") {

                if (stValue == "0") {
                    return false;
                }
                if (stValue == "1") {
                    return true;
                }

                return Convert.ToBoolean(stValue);
            }
            //TODO: type == "float" || type == "decimal" ||
            return stValue;
        }

        public static DateTime? HandleDateConversion(string stValue) {
            if (string.IsNullOrEmpty(stValue)) {
                return null;
            }

            int rowstamp;
            if (int.TryParse(stValue, out rowstamp)) {
                //TODO: change the whole rowstamp chain here, that´s being coverted from maximo side
                return null;
            }

            var kind = (ApplicationConfiguration.IsISM() || WsUtil.Is71() )? DateTimeKind.Utc : DateTimeKind.Local;
            var user = SecurityFacade.CurrentUser(false);
            try {
                var dateFromJson = Convert.ToDateTime(stValue);
                var date = DateTime.SpecifyKind(dateFromJson, kind);
                var fromUserToRightKind = date.FromUserToRightKind(user);
                return fromUserToRightKind;
            } catch (Exception) {
                DateTime dtAux;
                if (DateTime.TryParseExact(stValue, "MM/dd/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out dtAux)) {
                    var date = DateTime.SpecifyKind(dtAux, kind);
                    return date.FromUserToRightKind(user);
                }
                if (DateTime.TryParseExact(stValue, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out dtAux)) {
                    var date = DateTime.SpecifyKind(dtAux, kind);
                    return date.FromUserToRightKind(user);
                }
                if (DateTime.TryParseExact(stValue, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dtAux)) {
                    var date = DateTime.SpecifyKind(dtAux, kind);
                    return date.FromUserToRightKind(user);
                }

                if (DateTime.TryParseExact(stValue, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out dtAux)) {
                    var date = DateTime.SpecifyKind(dtAux, kind);
                    return date.FromUserToRightKind(user);
                }
                Log.WarnFormat("could not convert date for {0}", stValue);
                return null;
            }

        }
    }
}
