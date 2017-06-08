using cts.commons.portable.Util;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using System;
using cts.commons.simpleinjector;
using log4net;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Metadata.Properties;

namespace softWrench.sW4.Util {
    public static class DateExtensions {

        private static readonly ILog Log = LogManager.GetLogger(SwConstants.DATETIME_LOG);

        public static double ToTimeInMillis(this DateTime time) {
            return time.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        private static IConfigurationFacade Facade() {
            if (SimpleInjectorGenericFactory.Instance == null) {
                return null;
            }
            return SimpleInjectorGenericFactory.Instance
                .GetObject<IConfigurationFacade>();
        }



        /// <summary>
        /// Gets the Unix Time Stamp for the DateTime 
        /// (number of seconds passed since Unix Epoch = midnight 1/1/1970).
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUnixTimeStamp(this DateTime dateTime) {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var unixDateTime = (dateTime.ToUniversalTime() - epoch).TotalMilliseconds;
            return (long)Math.Truncate(unixDateTime);
        }

        /// <summary>
        /// Converts a Unix Time Stamp to a local DateTime.
        /// Has to be called statically from this class 
        /// (not possible to add extension static methods to classes, otherwise would have added it to DateTime).
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime FromUnixTimeStamp(long unixTimeStamp) {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var dateTime = epoch.AddSeconds(unixTimeStamp);
            return dateTime.ToLocalTime();
        }

        /// <summary>
        /// Converts a server DateTime to a client DateTime
        /// </summary>
        /// <param name="date">Server DateTime</param>
        /// <returns>Client DateTime</returns>     
        public static DateTime ToUserTimezone(this DateTime date, InMemoryUser user) {
            if (user == null || !user.TimezoneOffset.HasValue) {
                return date;
            }
            // ServerTime - UTCTime (in minutes)
            double serverOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;

            // UTCTime - ClientTime (in minutes)
            var clientOffset = user.TimezoneOffset.Value;

            // (ServerTime - UTCTime) + (UTCTime - ClientTime) == (ServerTime - ClientTime) * -1 == (ClientTime - ServerTime)
            var offset = (serverOffset + clientOffset) * -1;

            // ServerTime + (ClientTime - ServerTime) == ClientTime
            date = date.AddMinutes(offset);
            return date;
        }

        public static DateTime FromMaximoToUser(this DateTime date, InMemoryUser user) {
            return UserMaximoConversion(date, user, ConversionKind.MaximoToUser);
        }

        public static DateTime NowUnspecified() {
            var date = DateTime.Now;
            var newdate = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
            return newdate;
        }

        /// <summary>
        /// Converts a client DateTime to a server DateTime
        /// </summary>
        /// <param name="date">Client DateTime</param>
        /// <param name="user">current user</param>
        /// <returns>Server DateTime</returns>     
        public static DateTime FromUserToMaximo(this DateTime date, InMemoryUser user) {
            return UserMaximoConversion(date, user, ConversionKind.UserToMaximo);
        }

        public static DateTime FromUserToServer(this DateTime date, InMemoryUser user) {
            return UserMaximoConversion(date, user, ConversionKind.UserToMaximo, 0);
        }

        public static DateTime FromUserToRightKind(this DateTime date, InMemoryUser user) {
            var kind = (ApplicationConfiguration.IsISM() || WsUtil.Is71()) ? DateTimeKind.Utc : DateTimeKind.Local;
            if (WsUtil.Is75OrNewer()) {
                kind = DateTimeKind.Unspecified;
            }
            date = DateTime.SpecifyKind(date, kind);
            if (kind.Equals(DateTimeKind.Utc)) {
                return date.FromUserToUtc();
            }
            return FromUserToMaximo(date, user);
        }

        public static DateTime FromServerToRightKind(this DateTime date) {

            var kind = (ApplicationConfiguration.IsISM() || WsUtil.Is71()) ? DateTimeKind.Utc : DateTimeKind.Local;
            if (WsUtil.Is75OrNewer()) {
                kind = DateTimeKind.Unspecified;
            }
            date = DateTime.SpecifyKind(date, kind);
            if (kind.Equals(DateTimeKind.Utc)) {
                return FromServerToMaximo(date, 0);
            }
            return FromServerToMaximo(date);
        }

        public static DateTime FromUserToUtc(this DateTime date) {
            return date.ToUniversalTime();
        }

        public static DateTime FromUTCToUser(this DateTime date, InMemoryUser user) {
            if (!user.TimezoneOffset.HasValue) {
                return date;
            }
            return MaximoConversion(date, user.TimezoneOffset.Value, ConversionKind.MaximoToUser, 0);
        }

        public static DateTime FromServerToUser(this DateTime date, InMemoryUser user) {
            if (!user.TimezoneOffset.HasValue) {
                return date;
            }
            return MaximoConversion(date, user.TimezoneOffset.Value, ConversionKind.MaximoToUser, 0);
        }

        public static DateTime FromServerToMaximo(this DateTime date, int? maximoOverridenOffset = null) {
            //just needed to inver the timezone so that it is consistent witht the client signal
            return MaximoConversion(date, TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes * -1, ConversionKind.ServerToMaximo, maximoOverridenOffset);
        }

        public static DateTime FromMaximoToServer(this DateTime date) {
            return MaximoConversion(date, TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes * -1, ConversionKind.MaximoToServer);
        }

        public static DateTime FromUTCToMaximo(this DateTime date) {
            return MaximoConversion(date, 0, ConversionKind.UserToMaximo, null);
        }

        //to allow testing
        internal static DateTime UserMaximoConversion(DateTime date, InMemoryUser user, ConversionKind fromUserToMaximo, int? maximoOffset = null) {
            if (user == null || !user.TimezoneOffset.HasValue) {
                //if the user has no timezone there´s really nothing that we can do --> just return the date
                return date;
            }
            return MaximoConversion(date, user.TimezoneOffset.Value, fromUserToMaximo, maximoOffset);
        }

        internal static DateTime MaximoConversion(DateTime date, double offSet, ConversionKind kind, int? overridenMaximoOffSet = null) {
            var maximoOffset = 0.0;

            if (overridenMaximoOffSet == null) {
                var facade = Facade();
                string maximoTimezone;
                if (facade == null || ApplicationConfiguration.IsUnitTest) {
                    maximoTimezone = MetadataProvider.GlobalProperties.MaximoTimeZone();
                } else {
                    maximoTimezone = facade.Lookup<string>(ConfigurationConstants.Maximo.MaximoTimeZone, "maximoutc");
                }

                if (maximoTimezone != null) {
                    try {
                        var maximoTimezoneinfo = TimeZoneInfo.FindSystemTimeZoneById(maximoTimezone);
                        maximoOffset = maximoTimezoneinfo.GetUtcOffset(DateTime.UtcNow).TotalMinutes;
                    } catch (Exception) {
                        throw new MetadataException("wrong maximo utc property was set, review your properties.xml file");
                    }
                } else {
                    //if no property is present, let´s assume that both maximo and server are located under the same timezone
                    maximoOffset = Convert.ToInt32(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes);
                }
            } else {
                //for testing purposes, making it easier to mock the value that would be present on properties.xml
                maximoOffset = overridenMaximoOffSet.Value * 60;
            }

            var clientOffset = offSet;
            //ex: -7*60 +(180) ==> client timezone is positive...
            var offset = (maximoOffset + clientOffset);
            if (ConversionKind.MaximoToUser == kind || ConversionKind.MaximoToServer == kind) {
                offset = -1 * offset;
            }
            Log.Debug(string.Format("Input date: {0}  Input kind: {1}  Input offset: {2}  Output offset: {3}", date, kind, clientOffset, maximoOffset));
            date = date.AddMinutes(offset);
            if (WsUtil.Is75OrNewer()) {
                //TODO: is this ever needed again?
                date = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
            }
            Log.Debug(string.Format("Output date: {0}", date));
            return date;
        }



        /// <summary>
        /// Converts a client DateTime to a server DateTime
        /// </summary>
        /// <param name="date">Client DateTime</param>
        /// <returns>Server DateTime</returns>     
        public static DateTime ToServerTimezone(this DateTime date, InMemoryUser user) {
            if (user == null || !user.TimezoneOffset.HasValue) {
                return date;
            }
            // ServerTime - UTCTime (in minutes)
            double serverOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;

            // UTCTime - ClientTime (in minutes)
            var clientOffset = user.TimezoneOffset.Value;

            // (ServerTime - UTCTime) + (UTCTime - ClientTime) == (ServerTime - ClientTime)
            var offset = (serverOffset + clientOffset);

            // ClientTime + (ServerTime - ClientTime) == ServerTime
            date = date.AddMinutes(offset);


            return date;
        }

        /// <summary>
        /// Adds the given number of business days to the <see cref="DateTime"/>.
        /// </summary>
        /// <param name="current">The date to be changed.</param>
        /// <param name="days">Number of business days to be added.</param>
        /// <returns>A <see cref="DateTime"/> increased by a given number of business days.</returns>
        public static DateTime AddBusinessDays(this DateTime current, int days) {
            var sign = Math.Sign(days);
            var unsignedDays = Math.Abs(days);
            for (var i = 0; i < unsignedDays; i++) {
                do {
                    current = current.AddDays(sign);
                }
                while (current.DayOfWeek == DayOfWeek.Saturday ||
                    current.DayOfWeek == DayOfWeek.Sunday);
            }
            return current;
        }

        /// <summary>
        /// Subtracts the given number of business days to the <see cref="DateTime"/>.
        /// </summary>
        /// <param name="current">The date to be changed.</param>
        /// <param name="days">Number of business days to be subtracted.</param>
        /// <returns>A <see cref="DateTime"/> increased by a given number of business days.</returns>
        public static DateTime SubtractBusinessDays(this DateTime current, int days) {
            return AddBusinessDays(current, -days);
        }

        public static DateComparisonExpression IsOlderThan(this DateTime? date, int number, DateTime? toCompare = null) {
            if (date == null) {
                return new DateComparisonExpression();
            }
            if (toCompare == null) {
                toCompare = DateTime.Now;
            }

            return new DateComparisonExpression(date.Value, number, true, toCompare.Value);
        }

        internal enum ConversionKind {
            MaximoToUser, UserToServer, UserToMaximo, ServerToMaximo, MaximoToServer
        }

        public class DateComparisonExpression {
            DateTime _date;
            private int _amount;
            private bool _past;
            private DateTime _toCompare;

            public DateComparisonExpression() {

            }

            public DateComparisonExpression(DateTime date, int amount, bool past, DateTime toCompare) {
                _date = date;
                _amount = amount;
                _past = past;
                _toCompare = toCompare;
            }

            public virtual Boolean Days() {
                _toCompare = _toCompare.AddDays(_past ? -1 * _amount : _amount);
                _date = DateUtil.BeginOfDay(_date);
                _toCompare = DateUtil.BeginOfDay(_toCompare);
                return _date < _toCompare;
            }
        }




    }
}
