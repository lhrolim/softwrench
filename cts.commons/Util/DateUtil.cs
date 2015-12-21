using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace cts.commons.portable.Util {

    public static class DateUtil {

        // added additional acceptance format 
        public static readonly string[] FormatOptions = { "yyyy/MM/dd", "yyyy/MM/dd hh:mm", "dd/MM/yyyy hh:mm", "d/M/yyyy hh:mm", "MM/dd/yyyy hh:mm", "M/d/yyyy hh:mm", "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy", "M/d/yyy/", "yyyy-MM-dd", "yyyy-M-d" };

        public static DateTime? Parse(string date) {
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
    }
}
