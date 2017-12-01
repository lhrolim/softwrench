using System;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.util {
    public class FirstSolarOfflineMenuService : ISingletonComponent {

        private const string DateFormat = "MM/dd";

        public string BuildWoMenuContainerTitle() {
            var weekStartFormated = DefaultValuesBuilder.GetDefaultValue("@past(1week)", null, DateFormat); // monday 00:00:00

            var weekEnd = DateUtil.ParsePastAndFuture("1week", 1);
            weekEnd = weekEnd.AddSeconds(-1); // sunday 23:59:59
            var weekEndFormated = DefaultValuesBuilder.GetDateTimeAsString(weekEnd, DateFormat);

            return weekStartFormated + " - " + weekEndFormated;
        }


        public string BuildWeekTitle() {
            var weekStartFormated = DefaultValuesBuilder.GetDefaultValue("@past(1week)", null, DateFormat); // monday 00:00:00

            var weekEnd = DateUtil.ParsePastAndFuture("1week", 1);
            weekEnd = weekEnd.AddSeconds(-1); // sunday 23:59:59
            var weekEndFormated = DefaultValuesBuilder.GetDateTimeAsString(weekEnd, DateFormat);

            return weekStartFormated + " - " + weekEndFormated;
        }

        public string BuildMonthTitle() {
            var now = DateTime.Now;

            var monthEnd = now.LastDayOfMonth();

            var monthBegin = new DateTime(now.Year, now.Month, 1);

            return DefaultValuesBuilder.GetDateTimeAsString(monthBegin, DateFormat) + " - " + DefaultValuesBuilder.GetDateTimeAsString(monthEnd, DateFormat);
        }
    }
}
