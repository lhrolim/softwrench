using System.Collections.Generic;

namespace softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces {
    public interface IPropertyHolder {

        IDictionary<string, string> Properties { get; set; }
        
    }

    public static class PropertyHolderExtensions {


        public static IDictionary<string, string> MergeProperties(this IPropertyHolder source,
            IPropertyHolder overriden) {
            IDictionary<string, string> overridenParameters = new Dictionary<string, string>();

            foreach (var parameter in source.Properties) {
                var value = parameter.Value;
                if (overriden.Properties.ContainsKey(parameter.Key)) {
                    value = overriden.Properties[parameter.Key];
                }
                overridenParameters[parameter.Key] = value;
            }
            foreach (var parameter in overriden.Properties) {
                if (!overridenParameters.ContainsKey(parameter.Key)) {
                    overridenParameters[parameter.Key] = parameter.Value;
                }
            }

            source.Properties = overridenParameters;

            return overridenParameters;
        }

    }
}