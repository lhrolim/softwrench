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

        public IStereotype Merge(IStereotype stereotype) {
            var clonedProperties = new Dictionary<string, string>(Properties);

            foreach (var property in stereotype.StereotypeProperties()) {
                if (clonedProperties.ContainsKey(property.Key)) {
                    //overriding
                    clonedProperties[property.Key] = property.Value;
                } else {
                    //adding value that was not present
                    clonedProperties.Add(property.Key, property.Value);
                }
            }
            return new MetadataStereotype(((MetadataStereotype)stereotype).Id, clonedProperties);
        }


        public override string ToString() {
            return string.Format("Id: {0} Number of Commands: {1}", Id, Properties.Count);
        }

    }
}
