using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using softWrench.Mobile.Metadata.Parsing;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softWrench.Mobile.Metadata.Extensions {
    internal static class CompleteApplicationMetadataDefinitionExtension {

        //        public static ApplicationMetadataDefinition GetMetadata(this CompleteApplicationMetadataDefinition definition) {
        //            var mobileSchema = BuildMobileSchema(definition.Schemas);
        //            return new ApplicationMetadataDefinition(
        //                definition.Id,
        //                definition.Name,
        //                definition.Entity,
        //                definition.Title,
        //                definition.IdFieldName,
        //                mobileSchema);
        //        }

        public static ApplicationSchemaDefinition MobileSchema(this CompleteApplicationMetadataDefinition definition) {
            var schema = definition.ExtensionParameter("Schema");
            if (schema != null) {
                return (ApplicationSchemaDefinition)schema;
            }
            ApplicationSchemaDefinition mobileSchema = BuildMobileSchema(definition.SchemasList, definition.Parameters);
            definition.ExtensionParameter("Schema", mobileSchema);
            return mobileSchema;
        }

        private static ApplicationSchemaDefinition BuildMobileSchema(IEnumerable<ApplicationSchemaDefinition> schemas, IDictionary<string, string> parameters) {
            ApplicationSchemaDefinitionExtensions.Preview previewTitle = null, previewSubtitle = null, previewExcerpt = null, previewFeatured = null;
            var detailKey = new ApplicationMetadataSchemaKey(ApplicationMetadataConstants.Detail, null, ClientPlatform.Mobile);
            //            ApplicationSchemaDefinition listSchema, detailSchema;
            var applicationSchemaDefinitions = schemas as ApplicationSchemaDefinition[] ?? schemas.ToArray();
            var detailSchema = applicationSchemaDefinitions.FirstOrDefault(f => Equals(f.GetSchemaKey(), detailKey));
            var listSchema = applicationSchemaDefinitions.FirstOrDefault(f => Equals(f.GetSchemaKey(), new ApplicationMetadataSchemaKey(ApplicationMetadataConstants.List, null, ClientPlatform.Mobile)));
            if (detailSchema == null) {
                //should never happen while this class exists... only list/detail schema allowed for now
                return null;
            }
            if (listSchema != null) {
                detailSchema.PreviewTitle(BuildPreview(listSchema.Fields.FirstOrDefault(r => r.Qualifier == ApplicationMetadataConstants.PreviewTitle)));
                detailSchema.PreviewSubtitle(BuildPreview(listSchema.Fields.FirstOrDefault(r => r.Qualifier == ApplicationMetadataConstants.PreviewSubTitle)));
                detailSchema.PreviewFeatured(BuildPreview(listSchema.Fields.FirstOrDefault(r => r.Qualifier == ApplicationMetadataConstants.PreviewFeatured)));
                detailSchema.PreviewExcerpt(BuildPreview(listSchema.Fields.FirstOrDefault(r => r.Qualifier == ApplicationMetadataConstants.PreviewExcerpt)));
                var hiddenFields = listSchema.Fields.Where(f => f.IsHidden);
                foreach (var hiddenField in hiddenFields) {
                    if (!detailSchema.Displayables.Contains(hiddenField)) {
                        //All of the hidden fields could be useful for making the relationships on the single mobile schema
                        detailSchema.Displayables.Add(hiddenField);
                    }
                }
            }
//            ConvertAssociationsToLookups(detailSchema.ApplicationName, detailSchema.Displayables);

            string userInteractionEnabled;
            parameters.TryGetValue(ApplicationMetadataConstants.IsUserInteractionEnabledProperty, out userInteractionEnabled);
            //            detailSchema.IsUserInteractionEnabled(userInteractionEnabled != null && bool.Parse(userInteractionEnabled));
            return detailSchema;
        }

        //          <association label="Labor" target="laborcode" labelfield="labor.laborcode,person_.displayname" />
        //          <field attribute="labor_.laborcode" label="Labor" >
        //            <lookup sourceApplication="labor" sourceField="laborcode" sourceDisplay="laborcode,person_.displayname" targetField="laborcode" targetQualifier="">
        //              <lookupFilters >
        //                <lookupFilter sourceField="orgid" targetField="orgid" />
        //              </lookupFilters>
        //            </lookup>
        //          </field>

        private static void ConvertAssociationsToLookups(String applicationName,
            IList<IApplicationDisplayable> displayables) {
            var idxsToReplace = new Dictionary<int, ApplicationFieldDefinition>();
            for (int i = 0; i < displayables.Count; i++) {
                var displayable = displayables[i];
                var association = displayable as ApplicationAssociationDefinition;
                if (association == null) {
                    continue;
                }
                var attrs = association.EntityAssociation.NonPrimaryAttributes();
                var primaryAttr = association.EntityAssociation.PrimaryAttribute();
                var filters = new List<LookupWidgetDefinition.Filter>();
                foreach (var attribute in attrs) {
                    filters.Add(new LookupWidgetDefinition.Filter(attribute.To, attribute.From, attribute.Literal));
                }
                var attributeName = association.LabelFields[0];
                var lookup = new LookupWidgetDefinition(association.ApplicationTo, primaryAttr.To, new List<String>() { attributeName, primaryAttr.To }, primaryAttr.From, "", filters);

                var field =
                    displayables.OfType<ApplicationFieldDefinition>()
                        .FirstOrDefault(d => d.Attribute == association.ApplicationTo + "." + attributeName);
                if (field == null) {
                    field = new ApplicationFieldDefinition(applicationName, attributeName,
                        association.Label, false, false, false, new FieldRenderer(), lookup, association.DefaultValue,
                        null, null, association.ToolTip,null);
                    idxsToReplace.Add(i, field);
                } else {
                    field.WidgetDefinition = lookup;
                }
            }
            foreach (var idx in idxsToReplace.Keys) {
                displayables[idx] = idxsToReplace[idx];
            }

        }

        private static ApplicationSchemaDefinitionExtensions.Preview BuildPreview(ApplicationFieldDefinition field) {
            if (field == null) {
                return null;
            }
            return new ApplicationSchemaDefinitionExtensions.Preview(field.Label, field.Attribute);
        }

        public static string ToJson(this CompleteApplicationMetadataDefinition applicationSchemaDefinition) {
            return JsonConvert.SerializeObject(applicationSchemaDefinition, JsonParser.SerializerSettings);
            //            return new JsonApplicationMetadataParser().ToJson(ApplicationSchemaDefinition);
        }

    }
}
