using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using System.Collections.Generic;

namespace softWrench.sW4.Metadata.Stereotypes {

    class StereotypeFactory {

        private static IStereotype LookupStereotype(string stereotype, SchemaMode? mode) {
            if (stereotype.ToLower() == "list") {
                return ListSchemaStereotype.GetInstance();
            }
            if (stereotype.ToLower() == "detail") {
                if (SchemaMode.output.Equals(mode)) {
                    return OutputDetailStereotype.GetInstance();
                }
                return DetailSchemaStereotype.GetInstance();
            }

            if (stereotype.ToLower() == "compositionlist") {
                return CompositionListStereotype.GetInstance();
            }
            if (stereotype.ToLower() == "compositiondetail") {
                return CompositionDetailStereotype.GetInstance();
            }
            return new BlankStereotype();
        }

        internal static IStereotype LookupStereotype(SchemaStereotype type, SchemaMode? mode) {
            return LookupStereotype(type.ToString(), mode);
        }

        class BlankStereotype : IStereotype {

            public IDictionary<string, string> StereotypeProperties() {
                return new Dictionary<string, string>();
            }
        }

    }
}
