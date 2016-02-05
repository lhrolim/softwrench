using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Metadata.Stereotypes.Schema;

namespace softWrench.sW4.Metadata.Parsing {
    public class XmlEnabledFieldsVerifier {

        public static void VerifyEnabledFields(ApplicationSchemaDefinition schema) {
            var enabledFields = GetEnabledFields(schema);
            if (enabledFields == null) {
                return;
            }

            var localSchema = schema;
            while (localSchema != null) {
                InnerVerifyEnabledFields(enabledFields, localSchema.Displayables);
                localSchema = localSchema.ParentSchema;
            }
        }

        public static void VerifyEnabledField(ApplicationSchemaDefinition schema, IApplicationDisplayable displayable) {
            var enabledFields = GetEnabledFields(schema);
            if (enabledFields != null) {
                InnerVerifyEnabledField(enabledFields, displayable);
            }
        }

        private static List<string> GetEnabledFields(ApplicationSchemaDefinition schema) {
            var enabledFields = (string)null;
            schema.Properties.TryGetValue(ApplicationSchemaPropertiesCatalog.DetailEnabledFields, out enabledFields);
            if (enabledFields == null) {
                return null;
            }
            var enabledFieldsArray = enabledFields.Split(',');
            return enabledFieldsArray.Select(e => e.Trim()).ToList();
        }

        private static void InnerVerifyEnabledFields(List<string> enabledFields, List<IApplicationDisplayable> displayables) {
            if (displayables == null) {
                return;
            }
            displayables.ForEach(d => InnerVerifyEnabledField(enabledFields, d));
        }

        private static void InnerVerifyEnabledField(List<string> enabledFields, IApplicationDisplayable displayable) {
            var field = displayable as BaseApplicationFieldDefinition;
            if (field != null) {
                field.IsReadOnly = !IsEnabled(enabledFields, field.Attribute);
                return;
            }

            var container = displayable as IApplicationDisplayableContainer;
            if (container != null) {
                InnerVerifyEnabledFields(enabledFields, container.Displayables);
                return;
            }

            var association = displayable as ApplicationAssociationDefinition;
            if (association != null) {
                var isEnabled = IsEnabled(enabledFields, association.Target);
                association.EnableExpression = isEnabled ? "true" : "false";
                // turn to label because as now associations can not be read only
                association.RendererType = isEnabled ? association.RendererType : "label";
                return;
            }

            var composition = displayable as ApplicationCompositionDefinition;
            if (composition != null && composition.Schema != null) {
                var isEnabled = IsEnabled(enabledFields, composition.Attribute);
                var collectionSchema = composition.Schema as ApplicationCompositionCollectionSchema;
                if (collectionSchema == null || isEnabled) {
                    return;
                }
                collectionSchema.CollectionProperties.AllowInsertion = "false";
                collectionSchema.CollectionProperties.AllowUpdate = "false";
                return;
            }
        }

        private static bool IsEnabled(IEnumerable<string> enabledFields, string attribute) {
            return enabledFields.Any(e => e.Equals(attribute));
        }
    }
}
