using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid.Tags;
using JetBrains.Annotations;

namespace softWrench.sW4.Metadata.Stereotypes {

    public class MetadataStereotype : IStereotype {

        [NotNull]
        public string Id {
            get; private set;
        }

        public MetadataStereotype(string id, [NotNull]IDictionary<string, string> properties) {
            Id = id;
            Properties = properties;
        }

        [NotNull]
        public IDictionary<string, string> Properties {
            get; set;
        }

        public IDictionary<string, string> StereotypeProperties() {
            return Properties;
        }

        public void Merge(IStereotype stereotype) {
            foreach (var property in stereotype.StereotypeProperties()) {
                if (Properties.ContainsKey(property.Key)) {
                    //overriding
                    Properties[property.Key] = property.Value;
                } else {
                    //adding value that was not present
                    Properties.Add(property);
                }
            }
        }
    }
}
