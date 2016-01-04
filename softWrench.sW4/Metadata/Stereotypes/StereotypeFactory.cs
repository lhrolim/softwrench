using cts.commons.portable.Util;
using softWrench.sW4.Security.Init;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace softWrench.sW4.Metadata.Stereotypes {

    class StereotypeFactory {

        private static IStereotype LookupStereotype(string stereotype, SchemaMode? mode) {

            if (stereotype.ToLower() == "detail" && SchemaMode.output.Equals(mode)) {
                return MetadataProvider.Stereotype("detail.output");
            }
            if (stereotype.EqualsIc("compositiondetail")) {
                return MetadataProvider.Stereotype("detail.composition");
            }

            if (stereotype.EqualsIc("compositionlist")) {
                return MetadataProvider.Stereotype("list.composition");
            }

            return MetadataProvider.Stereotype(stereotype);

        }

        internal static IStereotype LookupStereotype(SchemaStereotype type, SchemaMode? mode) {
            return LookupStereotype(type.ToString(), mode);
        }


        [NotNull]
        public static IDictionary<string, MetadataStereotype> MergeStereotypes(IDictionary<string, MetadataStereotype> globalStereotypes, IDictionary<string, MetadataStereotype> customerStereotypes) {

            //first pass, let´s just override whichever stereotype property was redeclared on the customer metadata
            foreach (var cs in customerStereotypes) {
                var id = cs.Key;
                if (globalStereotypes.ContainsKey(id)) {
                    globalStereotypes[id].Merge(cs.Value);
                } else {
                    globalStereotypes.Add(cs);
                }
            }

            return globalStereotypes;
        }
    }
}
