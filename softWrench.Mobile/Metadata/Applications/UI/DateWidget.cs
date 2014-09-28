using System;
using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softWrench.Mobile.Metadata.Applications.UI {
    public sealed class DateWidget : DateWidgetDefinition, IWidget
    {

        public DateWidget(DateWidgetDefinition definition):base(definition.Format,definition.Time,definition.Min,definition.Max)
        {
        }

        public string Type { get { return GetType().Name; } }

        string IWidget.Format(string value) {
            string uiValue;
            DateTime dateValue;

            if (false == DateTime.TryParse(value, out dateValue)) {
                return value;
            }

            if (DateWidgetDefinition.IsShortFormat(this)) {
                uiValue = Time
                    ? dateValue.ToString("g")
                    : dateValue.ToShortDateString();
            } else if (DateWidgetDefinition.IsLongFormat(this)) {
                uiValue = Time
                    ? dateValue.ToString("f")
                    : dateValue.ToLongDateString();
            } else {
                try {
                    uiValue = dateValue.ToString(Format);
                } catch (FormatException) {
                    uiValue = value;
                }
            }

            return uiValue;
        }

        public bool Validate(string value, out string error) {
            error = null;
            return true;
        }


        
    }
}