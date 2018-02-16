using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Configuration.Definitions {

    public class CombinedPropertyValue : IPropertyValue {



        public CombinedPropertyValue(SortedDictionary<ConditionMatchResult, PropertyValue> values) {
            var sb = new StringBuilder();
            if (values.Count == 1) {
                var propertyValue = values.First().Value;
                StringValue = propertyValue.StringValue;
                SystemStringValue = propertyValue.SystemStringValue;
                return;
            }

            var i = 0;

            foreach (var entry in values) {
                var matchR = entry.Key;
                var propertyValue = entry.Value;

                if (ConditionMatch.Exact.Equals(matchR.MatchType)) {
                    StringValue = propertyValue.StringValue;
                    SystemStringValue = propertyValue.SystemStringValue;
                    break;
                }

                if (propertyValue.AllowCombining == true || i == 0) {
                    //either first time or if combining is allowed
                    if (propertyValue.SystemStringValue != null && propertyValue.SystemStringValue.StartsWith("@") && string.IsNullOrEmpty(propertyValue.StringValue)) {
                        //no way to combine a method clause with a ordinary clause yet
                        SystemStringValue = propertyValue.SystemStringValue;
                        break;
                    }

                    sb.Append("(" + propertyValue.ActualValue + ")").Append(" AND ");
                }

                i++;
            }

            if (sb.Length != 0) {
                StringValue = sb.ToString().Substring(0, sb.Length - " AND ".Length);
            }



        }

        public string StringValue { get; set; }
        public string SystemStringValue { get; set; }
    }
}
