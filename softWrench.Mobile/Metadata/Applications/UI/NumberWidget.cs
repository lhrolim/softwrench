using System.Threading;
using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softWrench.Mobile.Metadata.Applications.UI {
    public sealed class NumberWidget : NumberWidgetDefinition,IWidget {


        public NumberWidget(NumberWidgetDefinition definition)
            : base(definition.Decimals, definition.Min, definition.Max) {

        }

        public string Type { get { return GetType().Name; } }



        public string Format(string value) {
            decimal valueAsDecimal;

            // If we can't even convert the value
            // to a decimal, let's simply return
            // the original value.
            if (false == decimal.TryParse(value, out valueAsDecimal)) {
                return value;
            }

            // Adjusts decimals digits.
            valueAsDecimal = decimal.Round(valueAsDecimal, Decimals);

            // Converts the value back to a string
            // using the current culture.
            return valueAsDecimal.ToString(Thread.CurrentThread.CurrentCulture);
        }

        public bool Validate(string value, out string error) {
            decimal valueAsDecimal;

            // If we can't even convert the value
            // to a decimal, there is no hope...
            if (false == decimal.TryParse(value, out valueAsDecimal)) {
                error = "Please provide a valid number.";
                return false;
            }

            // Adjusts decimals digits.
            valueAsDecimal = decimal.Round(valueAsDecimal, Decimals);

            // Ensure the value is inside the valid range.
            if (valueAsDecimal < (Min ?? decimal.MinValue) || valueAsDecimal > (Max ?? decimal.MaxValue)) {
                if (Min != null && Max != null) {
                    error = string.Format("Please provide a number between {0} and {1}.", Min, Max);
                } else {
                    error = Min != null
                        ? string.Format("Please provide a number equal or greater than {0}.", Min)
                        : string.Format("Please provide a number equal or lower than {0}.", Max);
                }
                return false;
            }

            error = null;
            return true;
        }

    }
}
