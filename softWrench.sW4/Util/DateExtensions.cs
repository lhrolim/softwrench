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
            return UserMaximoConversion(date, user, false);
        }

        /// <summary>
        /// Converts a client DateTime to a server DateTime
        /// </summary>
        /// <param name="date">Client DateTime</param>
        /// <param name="user">current user</param>
        /// <returns>Server DateTime</returns>     
        public static DateTime FromUserToMaximo(this DateTime date, InMemoryUser user) {
            return UserMaximoConversion(date, user, true);
        }

        public static DateTime FromUserToRightKind(this DateTime date, InMemoryUser user) {
            var kind = ApplicationConfiguration.IsISM() ? DateTimeKind.Utc : DateTimeKind.Local;
            if (kind.Equals(DateTimeKind.Utc)) {
                return date.FromUserToUtc(user);
            }
            return date.ToServerTimezone(user);
        }

        public static DateTime FromServerToRightKind(this DateTime date) {
            var kind = ApplicationConfiguration.IsISM() ? DateTimeKind.Utc : DateTimeKind.Local;
            //            date = DateTime.SpecifyKind(date, kind);
            if (kind.Equals(DateTimeKind.Utc)) {
                return date.ToUniversalTime();
            }
            return FromServerToMaximo(date);
        }

        public static DateTime FromUserToUtc(this DateTime date, InMemoryUser user) {
            return UserMaximoConversion(date, user, true, 0);
        }

        public static DateTime FromServerToMaximo(this DateTime date, int? maximoOverridenOffset = null) {
            //just needed to inver the timezone so that it is consistent witht the client signal
            return MaximoConversion(date, TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes * -1, true, maximoOverridenOffset);
        }

        public static DateTime FromMaximoToServer(this DateTime date) {
            return MaximoConversion(date, TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes * -1, false);
        }

        private static DateTime UserMaximoConversion(DateTime date, InMemoryUser user, bool fromUserToMaximo, int? maximoOffset = null) {
            if (user == null || !user.TimezoneOffset.HasValue) {
                return date;
            }
            return MaximoConversion(date, user.TimezoneOffset.Value, fromUserToMaximo, maximoOffset);
        }

        private static DateTime MaximoConversion(DateTime date, double offSet, bool fromUserToMaximo, int? overridenMaximoOffSet = null) {
            int maximoOffset = 0;
            if (overridenMaximoOffSet == null) {
                var maximoUtcProp = MetadataProvider.GlobalProperties.MaximoTimeZone();
                int maximoUtc;
                if (!Int32.TryParse(maximoUtcProp, out maximoUtc)) {
                    return date;
                }
                maximoOffset = maximoUtc * 60;
            }
            var clientOffset = offSet;
            //ex: -7*60 +(180) ==> client timezone is positive...
            var offset = (maximoOffset + clientOffset);
            if (!fromUserToMaximo) {
                offset = -1 * offset;
            }
            date = date.AddMinutes(offset);

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



    }
}
