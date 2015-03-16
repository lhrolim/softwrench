using cts.commons.portable.Util;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using System;

namespace softWrench.sW4.Util {
    public static class DateExtensions {

        public static double ToTimeInMillis(this DateTime time) {
            return time.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
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

        public static DateTime FromUserToRightKind(this DateTime date, InMemoryUser user) {
            var kind = ApplicationConfiguration.IsISM() ? DateTimeKind.Utc : DateTimeKind.Local;
            if (WsUtil.Is75()) {
                kind = DateTimeKind.Unspecified;
            }
            date = DateTime.SpecifyKind(date, kind);
            if (kind.Equals(DateTimeKind.Utc)) {
                return date.FromUserToUtc();
            }
            return FromUserToMaximo(date, user);
        }

        public static DateTime FromServerToRightKind(this DateTime date) {

            var kind = ApplicationConfiguration.IsISM() ? DateTimeKind.Utc : DateTimeKind.Local;
            if (WsUtil.Is75()) {
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

        public static DateTime FromServerToMaximo(this DateTime date, int? maximoOverridenOffset = null) {
            //just needed to inver the timezone so that it is consistent witht the client signal
            return MaximoConversion(date, TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes * -1, ConversionKind.ServerToMaximo, maximoOverridenOffset);
        }

        public static DateTime FromMaximoToServer(this DateTime date) {
            return MaximoConversion(date, TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes * -1, ConversionKind.MaximoToServer);
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
                var maximoTimezone = MetadataProvider.GlobalProperties.MaximoTimeZone();
                if (maximoTimezone != null) {
                    try {
                        var maximoTimezoneinfo = TimeZoneInfo.FindSystemTimeZoneById(maximoTimezone);
                        maximoOffset = maximoTimezoneinfo.GetUtcOffset(DateTime.UtcNow).TotalMinutes;
                    } catch (Exception e) {
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
            date = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
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
            MaximoToUser, UserToMaximo, ServerToMaximo, MaximoToServer
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
