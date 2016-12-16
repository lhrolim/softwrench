using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Imac {
    public class ImacDescriptionHandler {

        private const string SectionSeparator = "=====================================================================";

        private const string NoLabelFieldPattern = "{0}";
        private const string FieldPattern = "{0}:   {1}";
        private const string FieldOldNewPattern = "{0}:   {1} --> {2}";
        private const string NoDescriptionQualifier = "nodescription";
        private const string NewDescriptionQualifier = "_new";


        public static string BuildDescription(CrudOperationData jsonObject, ApplicationMetadata metadata) {
            var sb = new StringBuilder();
            sb.AppendLine();
            var applicationSections = metadata.Schema.GetDisplayable<ApplicationSection>(typeof(ApplicationSection), SchemaFetchMode.FirstLevelOnly);
            foreach (var section in applicationSections) {
                CreateSection(jsonObject, sb, section);
            }
            CloseSection(sb);

            return sb.ToString();
        }

        private static void CreateSection(CrudOperationData jsonObject, StringBuilder sb,
            ApplicationSection section) {
            OpenSection(sb, section);
            var displayables = DisplayableUtil.GetDisplayable<IApplicationAttributeDisplayable>(
                typeof(IApplicationAttributeDisplayable), section.Displayables);

            if (section.Id == "specification") {
                HandleSpecifications(sb, (string)jsonObject.GetAttribute(section.Id), section);
            } else {

                DoHandleDisplayables(jsonObject, sb, displayables);
            }
        }

        private static void DoHandleDisplayables(CrudOperationData jsonObject, StringBuilder sb, IList<IApplicationAttributeDisplayable> displayables) {
            foreach (var attributeDisplayable in displayables) {
                if (attributeDisplayable.Qualifier != null &&
                    (attributeDisplayable.Qualifier.Contains(NoDescriptionQualifier) ||
                     attributeDisplayable.Qualifier.EndsWith(NewDescriptionQualifier))) {
                    //these fields should not go into the description
                    continue;
                }
                if (attributeDisplayable is IApplicationDisplayableContainer) {
                    continue;
                }

                if (!jsonObject.ContainsAttribute(attributeDisplayable.Attribute)) {
                    continue;
                }

                if (attributeDisplayable.Attribute == "assetattributes") {
                    HandleAssetAttributes(sb, (string)jsonObject.GetAttribute(attributeDisplayable.Attribute),
                        attributeDisplayable);
                    continue;
                }

                if (attributeDisplayable.Attribute == "assetCommodities") {
                    HandleAssetCommodities(sb, (string)jsonObject.GetAttribute(attributeDisplayable.Attribute),
                        attributeDisplayable);
                    continue;
                }

                var oldValue = GetValue(jsonObject, attributeDisplayable);
                var newValue = LocateNewValue(attributeDisplayable.Attribute, jsonObject, displayables);
                AppendField(sb, attributeDisplayable.Label, oldValue, newValue);
            }
        }

        private static object GetValue(CrudOperationData jsonObject, IApplicationAttributeDisplayable attributeDisplayable) {
            string labelAttribute = "#" + attributeDisplayable.Attribute + "_label";
            if (jsonObject.ContainsAttribute(labelAttribute)) {
                return jsonObject.GetAttribute(labelAttribute);
            }
            return jsonObject.GetAttribute(attributeDisplayable.Attribute);
        }

        private static void HandleSpecifications(StringBuilder sb, string jsonString, IApplicationAttributeDisplayable attributeDisplayable) {
            if (jsonString == null) {
                return;
            }
            var arr = JArray.Parse("[" + jsonString + "]");
            foreach (var assetAttribute in arr) {
                var label = assetAttribute.Value<string>("label");
                var oldValue = assetAttribute.Value<string>("value");
                var newValue = assetAttribute.Value<string>("#newvalue");
                AppendField(sb, label, oldValue, newValue);
            }
        }

        private static void HandleAssetCommodities(StringBuilder sb, string jsonString, IApplicationAttributeDisplayable attributeDisplayable) {
            if (!String.IsNullOrWhiteSpace(jsonString)) {
                var arr = jsonString.Split(',');
                foreach (var assetAttribute in arr) {
                    sb.AppendLine(String.Format(NoLabelFieldPattern, assetAttribute.Trim()));
                }
            }
        }

        private static void HandleAssetAttributes(StringBuilder sb, string jsonString,
            IApplicationAttributeDisplayable attributeDisplayable) {
            if (jsonString == null) {
                return;
            }

            var arr = JArray.Parse("[" + jsonString + "]");
            foreach (var assetAttribute in arr) {
                var label = assetAttribute.Value<string>("assetattribute_.description");
                var oldValue = assetAttribute.Value<string>("value");
                var newValue = assetAttribute.Value<string>("#newvalue");
                AppendField(sb, label, oldValue, newValue);
            }
        }

        private static string LocateNewValue(string attribute, CrudOperationData jsonObject, IList<IApplicationAttributeDisplayable> displayables) {
            var targetQualifier = attribute + NewDescriptionQualifier;
            var newDisplayable = displayables.FirstOrDefault(a => a.Qualifier == targetQualifier);
            if (newDisplayable == null) {
                return null;
            }
            return GetValue(jsonObject,newDisplayable) as string;
        }

        public static void OpenSection(StringBuilder sb, ApplicationSection section) {
            sb.AppendLine(SectionSeparator);
            string sectionTitle;
            if (!section.Parameters.TryGetValueAsString("detaildescription", out sectionTitle)) {
                if (section.Header != null) {
                    sectionTitle = section.Header.Label;
                } else {
                    ExceptionUtil.InvalidOperation(
                        "unable to determine section title. Please specify parameter detaildescription or a header to section {0}",
                        section.Id);
                }
            }
            if (!String.IsNullOrWhiteSpace(sectionTitle)) {
                sb.AppendLine(sectionTitle);
                sb.AppendLine();
            }
        }

        public static void CloseSection(StringBuilder sb) {
            sb.AppendLine(SectionSeparator);
        }

        public static void AppendField(StringBuilder sb, string label, object oldValue, object newValue) {
            if (label.Equals("from Location")) {
                label = "Location";
            }
            if (newValue == null) {
                sb.AppendLine(String.Format(FieldPattern, label, oldValue));
            } else {
                sb.AppendLine(String.Format(FieldOldNewPattern, label, oldValue, newValue));
            }
        }



    }
}
