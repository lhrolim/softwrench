using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using cts.commons.portable.Util;
using log4net;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Util {
    public class ConversionUtil {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ConversionUtil));
        private static readonly List<string> DateFormats = new List<string>() {
            "MM/dd/yyyy", "dd/MM/yyyy HH:mm", "dd/MM/yyyy HH:mm:ss", "MM/dd/yyyy", "MM/dd/yyyy HH:mm", "MM/dd/yyyy HH:mm:ss"
        };


        public static object ConvertFromMetadataType(string type, string stValue, bool isSWDBDate) {
            //TODO: review.

            if (type == "varchar" || type == "string") {
                return stValue;
            }
            if (type.EqualsAny("smallint", "int", "integer")) {
                if (stValue.EqualsAny("True", "False")) {
                    return Convert.ToBoolean(stValue);
                }
                try {
                    return Convert.ToInt32(stValue);
                } catch (System.OverflowException) {
                    return Convert.ToInt64(stValue);
                }
            }
            if (type == "bigint") {
                return Convert.ToInt64(stValue);
            }
            if (type == "datetime" || type == "timestamp") {

                return HandleDateConversion(stValue, isSWDBDate);
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

        public static DateTime? HandleDateConversion(string stValue, bool isSwdbDate) {
            if (string.IsNullOrEmpty(stValue)) {
                return null;
            }

            long rowstamp;
            if (Int64.TryParse(stValue, out rowstamp)) {
                //TODO: change the whole rowstamp chain here, that´s being coverted from maximo side
                return null;
            }

            var kind = (ApplicationConfiguration.IsISM() || WsUtil.Is71()) ? DateTimeKind.Utc : DateTimeKind.Local;
            if (isSwdbDate) {
                kind = DateTimeKind.Local;
            }

            var user = SecurityFacade.CurrentUser(false);
            try {
                var dateFromJson = Convert.ToDateTime(stValue, new CultureInfo("en-US"));
                var date = DateTime.SpecifyKind(dateFromJson, kind);
                if (isSwdbDate) {
                    return date.FromUserToServer(user);
                }
                var fromUserToRightKind = date.FromUserToRightKind(user);
                return fromUserToRightKind;
            } catch (Exception) {
                foreach (var dateFormat in DateFormats) {
                    DateTime dtAux;
                    if (!DateTime.TryParseExact(stValue, dateFormat, null, System.Globalization.DateTimeStyles.None, out dtAux)) continue;
                    var date = DateTime.SpecifyKind(dtAux, kind);
                    if (isSwdbDate) {
                        return date.FromUserToServer(user);
                    }
                    return date.FromUserToRightKind(user);
                }

                Log.WarnFormat("could not convert date for {0}", stValue);
                return null;
            }

        }
    }
}
