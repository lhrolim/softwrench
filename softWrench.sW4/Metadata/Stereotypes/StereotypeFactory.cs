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
                return MetadataProvider.Stereotype("detailoutput");
            }
            //            if (stereotype.EqualsIc("compositiondetail")) {
            //                return MetadataProvider.Stereotype("detail.composition");
            //            }
            //
            //            if (stereotype.EqualsIc("compositionlist")) {
            //                return MetadataProvider.Stereotype("list.composition");
            //            }

            return MetadataProvider.Stereotype(stereotype);

        }

        internal static IStereotype LookupStereotype(SchemaStereotype type, SchemaMode? mode) {
            return LookupStereotype(type.ToString().ToLower(), mode);
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



            foreach (var gs in globalStereotypes) {
                var stereotypeProperties = gs.Value.StereotypeProperties();
                var keys = new List<string>(gs.Value.StereotypeProperties().Keys);
                foreach (var key in keys) {
                    var overridenValue = GetCustomClientValue(key);
                    if (overridenValue != null) {
                        stereotypeProperties[key] = overridenValue;
                    }

                }
            }

            return globalStereotypes;
        }


        private static string GetCustomClientValue(string key) {
            var globalProperties = MetadataProvider.GlobalProperties.Properties;
            string globalValue;
            if (globalProperties.TryGetValue(key, out globalValue)) {
                return globalValue;
            }
            return null;
        }
    }
}
