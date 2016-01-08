using cts.commons.portable.Util;
using softWrench.sW4.Security.Init;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using System.Collections.Generic;
using softWrench.sW4.Metadata.Stereotypes.Component;

namespace softWrench.sW4.Metadata.Stereotypes {

    class ComponentStereotypeFactory {

        private static IStereotype LookupStereotype(string stereotype) {
            if (stereotype == null) {
                return new BlankStereotype();
            }

            if (stereotype.ToLower() == "email") {
                return EmailComponentStereotype.GetInstance();
            }
            return new BlankStereotype();
        }

        internal static IStereotype LookupStereotype(ComponentStereotype type) {
            return LookupStereotype(type.ToString());
        }

        class BlankStereotype : IStereotype {

            public IDictionary<string, string> StereotypeProperties() {
                return new Dictionary<string, string>();
            }

            public IStereotype Merge(IStereotype stereotype) {
                return this;
            }
        }

    }
}
