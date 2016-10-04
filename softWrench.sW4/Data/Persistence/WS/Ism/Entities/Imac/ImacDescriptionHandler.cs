using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Util;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Imac {
    class ImacDescriptionHandler {

        private const string SectionSeparator = "=====================================================================";

        private const string NoLabelFieldPattern = "{0}";
        private const string FieldPattern = "{0}:   {1}";
        private const string FieldOldNewPattern = "{0}:   {1} --> {2}";
        private const string NoDescriptionQualifier = "nodescription";
        private const string NewDescriptionQualifier = "_new";


        public static string BuildDescription(CrudOperationData jsonObject, ApplicationMetadata metadata) {
            var sb = new StringBuilder();
            sb.AppendLine();
            var applicationSections = metadata.Schema.GetDisplayable<ApplicationSection>(typeof(ApplicationSection), false);
            foreach (var section in applicationSections) {
                if (!section.ShowExpression.Equals("false")) {
                    CreateSection(jsonObject, sb, section);
                }
            }
            CloseSection(sb);

            return sb.ToString();
        }

        private static void CreateSection(CrudOperationData jsonObject, StringBuilder sb,
            ApplicationSection section) {
            if (section.Parameters.ContainsKey("nodescription")) {
                return;
            }

            var sectionHeader = OpenSection(section);
            var displayables = DisplayableUtil.GetDisplayable<IApplicationAttributeDisplayable>(
                typeof(IApplicationAttributeDisplayable), section.Displayables,true,true);

            if (section.Id == "specification") {
                sb.Append(sectionHeader);
                HandleSpecifications(sb, (string)jsonObject.GetAttribute(section.Id), section);
                return;
            }
            var st = DoHandleDisplayables(jsonObject, displayables);
            if (!String.IsNullOrEmpty(st)) {
                sb.Append(sectionHeader);
                sb.Append(st);
            }
        }

        private static string DoHandleDisplayables(CrudOperationData jsonObject, IList<IApplicationAttributeDisplayable> displayables) {
            var sb = new StringBuilder();
            foreach (var attributeDisplayable in displayables) {
                if (attributeDisplayable.Qualifier != null && attributeDisplayable.Qualifier.Contains(NoDescriptionQualifier) || attributeDisplayable.ShowExpression == "false") {
                    continue;
                }


                if (attributeDisplayable is IApplicationDisplayableContainer) {

                    continue;
                }

                if (!jsonObject.ContainsAttribute(attributeDisplayable.Attribute) && !jsonObject.ContainsAttribute("#" + attributeDisplayable.Attribute + "_label")) {
                    continue;
                }

                if (attributeDisplayable.Attribute == "assetattributes") {
                    HandleAssetAttributes(sb, (string)jsonObject.GetAttribute(attributeDisplayable.Attribute),
                        attributeDisplayable);
                    continue;
                }



                if (attributeDisplayable.Attribute == "assetCommodities") {
                    HandleAssetCommodities(sb, (string)jsonObject.GetAttribute("#" + attributeDisplayable.Attribute + "_label"),
                        attributeDisplayable);
                    continue;
                }


                var oldValue = GetValue(jsonObject, attributeDisplayable);
                oldValue = HandlePrePend(oldValue as string, attributeDisplayable);

                var newValue = LocateNewValue(attributeDisplayable.Attribute, jsonObject, displayables);
                newValue = HandlePrePend(newValue as string, attributeDisplayable);


                if (attributeDisplayable.Qualifier != null &&
                    attributeDisplayable.Qualifier.EndsWith(NewDescriptionQualifier)) {
                    // we should ignore the old value label, if there´s a newValue present, since they will be combined
                    continue;
                }


                sb.AppendLine(AppendField(attributeDisplayable.Label, oldValue, newValue));
            }
            return sb.ToString();
        }

        private static string HandlePrePend(string value, IApplicationAttributeDisplayable attributeDisplayable) {
            if (value == null) {
                return value;
            }
            if (attributeDisplayable.RendererParameters.ContainsKey("prepend")) {
                value = attributeDisplayable.RendererParameters["prepend"] + value;
            }
            if (attributeDisplayable.RendererParameters.ContainsKey("append")) {
                value = value + attributeDisplayable.RendererParameters["append"];
            }
            return value;
        }

        private static object GetValue(CrudOperationData jsonObject, IApplicationAttributeDisplayable attributeDisplayable) {
            string labelAttribute = "#" + attributeDisplayable.Attribute + "_label";
            if (jsonObject.ContainsAttribute(labelAttribute)) {
                return jsonObject.GetAttribute(labelAttribute);
            }
            if (attributeDisplayable.RendererType != null && attributeDisplayable.RendererType.Equals("upload")) {
                return jsonObject.GetAttribute(attributeDisplayable.Attribute + "_path");
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
                sb.AppendLine(AppendField(label, oldValue, newValue));
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
                sb.AppendLine(AppendField(label, oldValue, newValue));
            }
        }

        private static string LocateNewValue(string attribute, CrudOperationData jsonObject, IList<IApplicationAttributeDisplayable> displayables) {
            var targetQualifier = attribute + NewDescriptionQualifier;
            var newDisplayable = displayables.FirstOrDefault(a => a.Qualifier == targetQualifier);
            if (newDisplayable == null) {
                return null;
            }
            return GetValue(jsonObject, newDisplayable) as string;
        }

        public static string OpenSection(ApplicationSection section) {
            var sb = new StringBuilder();
            sb.AppendLine(SectionSeparator);
            string sectionTitle;
            if (!section.Parameters.TryGetValue("detaildescription", out sectionTitle)) {
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
            return sb.ToString();
        }

        public static void CloseSection(StringBuilder sb) {
            sb.AppendLine(SectionSeparator);
        }

        public static string AppendField(string label, object oldValue, object newValue) {
            if ("from Location".Equals(label)) {
                label = "Location";
            }
            if (newValue == null || (newValue.Equals(oldValue) && "Mac Address".Equals(label))) {
                return String.Format(FieldPattern, label, oldValue);
            }
            return String.Format(FieldOldNewPattern, label, oldValue, newValue);
        }



    }
}
