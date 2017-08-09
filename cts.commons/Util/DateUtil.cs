using System;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace cts.commons.portable.Util {

    public static class DateUtil {

        // added additional acceptance format 
        //TODO: modify this solution, as dd/mmy/yyyy and mm/dd/yyyy should need to be choosen based upon a locale rule (03/10/2016 could be parsed, both ways)
        public static readonly string[] FormatOptions = { "yyyy/MM/dd", "yyyy/MM/dd hh:mm", "yyyy/MM/dd HH:mm", "yyyy/MM/dd hh:mm tt", "MM/dd/yyyy hh:mm", "MM/dd/yyyy hh:mm tt", "MM/dd/yyyy hh:mm:ss", "MM/dd/yy hh:mm:ss", "MM/dd/yyyy HH:mm:ss", "MM/dd/yy HH:mm:ss", "dd/MM/yyyy hh:mm", "d/M/yyyy hh:mm", "MM/dd/yyyy HH:mm", "M/d/yyyy hh:mm", "MM/dd/yyyy", "dd/MM/yyyy", "d/M/yyyy", "M/d/yyy/", "yyyy-MM-dd", "yyyy-MM-dd hh:mm", "yyyy-MM-dd HH:mm", "yyyy-M-d", "yyyy-MM-dd HH:mm:ss.FFF", "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFK" };

        public const string MaximoDefaultIntegrationFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFK";

        public const string DefaultUsaDateTimeFormat = "MM/dd/yyyy HH:mm";

        public static DateTime? ParseExact(string date, string preferredFormat) {
            DateTime temp;
            // Switched from TryParse - it accepted 4500-5

            var formatsToUse = new[] { preferredFormat };

            if (DateTime.TryParseExact(date, formatsToUse, CultureInfo.InvariantCulture, DateTimeStyles.None, out temp)) {
                return temp;
            }
            return null;
        }

        public static DateTime? Parse([CanBeNull]string date) {
            if (date == null) {
                return null;
            }

            DateTime temp;
            // Switched from TryParse - it accepted 4500-5

            if (DateTime.TryParseExact(date, FormatOptions, CultureInfo.InvariantCulture, DateTimeStyles.None, out temp)) {
                return temp;
            }
            return null;
        }

        public static DateTime BeginOfDay(DateTime date) {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
        }

        public static DateTime BeginOfToday() {
            var date = DateTime.Now;
            return BeginOfDay(date);
        }

        public static DateTime EndOfDay(DateTime date) {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        public static DateTime EndOfToday() {
            var date = DateTime.Now;
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
        }

        public static double TimeInMillis(this DateTime time) {
            return (time - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }




        /// <summary>
        /// Parse past and future dates given a string representation of time.
        /// Ex: "3 months", "1 year", "2 hours"
        /// TODO: increment this method to accept decimal values, like "0.5 hour"
        /// TODO: increment this method to accept composite time, like "1 year and 6 months"
        /// </summary>
        /// <param name="valueToParse">String representation of time</param>
        /// <param name="pastOrFuture">-1 for past, 1 for future </param>
        /// <returns>Date/time parsed</returns>
        public static DateTime ParsePastAndFuture(String valueToParse, int pastOrFuture) {

            if (pastOrFuture != 1 && pastOrFuture != -1) {
                throw new ArgumentException("pastOrFuture");
            }

            try {

                DateTime value = DateTime.Now;
                String[] splittedTime = Regex.Split(valueToParse, @"(?<=\d)(?=\D)");
                int number = Int32.Parse(splittedTime[0].Trim()) * pastOrFuture;
                string timeUnit = splittedTime[1].Trim().ToLower();

                switch (timeUnit) {
                    case "year":
                    case "years":
                    value = value.AddYears(number);
                    break;
                    case "month":
                    case "months":
                    value = value.AddMonths(number);
                    break;
                    case "week":
                    case "weeks":
                    value = ProcessWeeks(value, number);
                    break;
                    case "day":
                    case "days":
                    value = value.AddDays(number);
                    break;
                    case "hour":
                    case "hours":
                    value = value.AddHours(number);
                    break;
                    case "minute":
                    case "minutes":
                    value = value.AddMinutes(number);
                    break;
                    case "second":
                    case "seconds":
                    value = value.AddSeconds(number);
                    break;
                    default:
                    throw new FormatException();
                }
                return value;
            } catch (Exception) {
                throw new FormatException("String representation of time in a incorrect format: " + valueToParse);
            }
        }

        /// <summary>
        /// Process the number of weeks time. 
        /// If past and one returs the 0h of the last sunday. 
        /// If future and one returns the 0h of the next sunday. 
        /// For values higher and lower than one more weeks are added to the result.
        /// </summary>
        /// <param name="baseTime"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static DateTime ProcessWeeks(DateTime baseTime, int number) {
            if (number == 0) {
                return baseTime;
            }
            var future = number > 0;
            var dayOfWeek = baseTime.DayOfWeek;
            var dayOfWeekDelta = DayOfWeek.Sunday - dayOfWeek;
            if (future) {
                dayOfWeekDelta += 7;
            }

            var extraWeeks = future ? number - 1 : number + 1;
            var resultTime = baseTime.AddDays(dayOfWeekDelta + extraWeeks * 7);
            resultTime = resultTime.AddHours(-resultTime.Hour);
            resultTime = resultTime.AddMinutes(-resultTime.Minute);
            resultTime = resultTime.AddSeconds(-resultTime.Second);
            resultTime = resultTime.AddMilliseconds(-resultTime.Millisecond);
            return resultTime;
        }
    }
}
