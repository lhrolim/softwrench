using System;
using System.ComponentModel;

namespace softwrench.sW4.Shared2.Metadata.Applications.UI {
    public class DateWidgetDefinition : IWidgetDefinition {
        public const string ShortFormat = "short";
        public const string LongFormat = "long";

        private static string NormalizeFormat(string format) {
            if (IsShortFormat(format)) {
                return ShortFormat;
            }

            return IsLongFormat(format)
                ? LongFormat
                : format;
        }

        private static bool IsShortFormat(string format) {
            if (format == null) throw new ArgumentNullException("format");

            return format.Equals(ShortFormat, StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool IsLongFormat(string format) {
            if (format == null) throw new ArgumentNullException("format");

            return format.Equals(LongFormat, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool IsShortFormat(DateWidgetDefinition widgetDefinition) {
            if (widgetDefinition == null) throw new ArgumentNullException("widgetDefinition");

            return IsShortFormat(widgetDefinition.Format);
        }

        public static bool IsLongFormat(DateWidgetDefinition widgetDefinition) {
            if (widgetDefinition == null) throw new ArgumentNullException("widgetDefinition");

            return IsLongFormat(widgetDefinition.Format);
        }

        public DateWidgetDefinition(string format, bool time, DateTime min, DateTime max) {
            if (format == null) throw new ArgumentNullException("format");

            Format = NormalizeFormat(format);
            Time = time;
            Min = min;
            Max = max;
        }

        [DefaultValue(ShortFormat)]
        public string Format { get; set; }

        public bool Time { get; set; }

        public DateTime Min { get; set; }

        public DateTime Max { get; set; }

        // manually specifying serialization rules for the dates since the desired default values are not constants 
        // (cant be passed as attribute/annotation arguments)
        // source: http://www.newtonsoft.com/json/help/html/conditionalproperties.htm
        public bool ShouldSerializeMin() { return Min != DateTime.MinValue; }
        public bool ShouldSerializeMax() { return Max != DateTime.MaxValue; }

        public string Type {
            get { return GetType().Name; }
        }

        public bool IsShortFormat() {
            return IsShortFormat(this);
        }

        public bool IsLongFormat() {
            return IsLongFormat(this);
        }
    }
}