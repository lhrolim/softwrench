using System;
using cts.commons.portable.Util;
using softWrench.sW4.Security.Init;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace softWrench.sW4.Metadata.Stereotypes {

    class StereotypeFactory {


        public static SchemaStereotype ParseStereotype(string stereotypeAttr) {
            SchemaStereotype stereotype = SchemaStereotype.None;
            var result = Enum.TryParse(stereotypeAttr, true, out stereotype);
            if (!result) {
                if (stereotypeAttr.Contains("detail")) {
                    if (stereotypeAttr.Contains("new")) {
                        return SchemaStereotype.DetailNew;
                    }
                    return SchemaStereotype.Detail;
                }
                if (stereotypeAttr.Contains("list")) {
                    return SchemaStereotype.List;
                }
            }
            return stereotype;
        }

        public static IStereotype LookupStereotype([CanBeNull]string stereotype, SchemaMode? mode) {
            if (stereotype == null)
            {
                return MetadataProvider.Stereotype(null);
            }

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



        [NotNull]
        public static IDictionary<string, MetadataStereotype> MergeStereotypes(IDictionary<string, MetadataStereotype> globalStereotypes, IDictionary<string, MetadataStereotype> customerStereotypes) {

            //first pass, let´s just override whichever stereotype property was redeclared on the customer metadata
            foreach (var cs in customerStereotypes) {
                var id = cs.Key;
                if (globalStereotypes.ContainsKey(id)) {
                    globalStereotypes[id] = (MetadataStereotype)globalStereotypes[id].Merge(cs.Value);
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
