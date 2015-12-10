using JetBrains.Annotations;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Metadata.Applications;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data {

    /// <summary>
    /// A Datamap where each attribute is a comma sepparated list of strings, that can be used for in-like queries
    /// </summary>
    public class CompositeDatamap : AttributeHolder {

        private readonly string _applicationName;


        public CompositeDatamap(IEnumerable<AttributeHolder> compositionAttributes) {
            Attributes = new Dictionary<string, object>();
            foreach (var compositionData in compositionAttributes) {
                _applicationName = compositionData.HolderName();
                AppendComposition(compositionData);
            }
        }

        private void AppendComposition(AttributeHolder datamapToAppend) {
            foreach (var attribute in datamapToAppend.Attributes) {
                if (!Attributes.ContainsKey(attribute.Key)) {
                    if (attribute.Value == null) {
                        continue;
                    }

                    var value = attribute.Value as string;
                    if (value == null) {
                        value = attribute.Value.ToString();
                    }
                    if (!string.IsNullOrEmpty(value)) {
                        Attributes.Add(attribute.Key, value);
                    }
                } else {
                    var originalAttribute = Attributes[attribute.Key] as string;
                    if (!string.IsNullOrEmpty(attribute.Value as string)) {
                        Attributes[attribute.Key] = originalAttribute + "," + attribute.Value;
                    }
                }
            }
        }


        public override string HolderName() {
            return _applicationName;
        }
    }
}
