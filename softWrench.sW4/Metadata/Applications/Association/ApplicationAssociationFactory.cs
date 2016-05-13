using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Util;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softWrench.sW4.Metadata.Stereotypes;

namespace softWrench.sW4.Metadata.Applications.Association {
    public class ApplicationAssociationFactory {

        public static ApplicationAssociationDefinition GetFilterInstance(string from, string labelField, string target) {
            var schema = new ApplicationAssociationSchemaDefinition(new AssociationDataProvider(), new AssociationFieldRenderer(), null);
            return GetInstance(from, new ApplicationAssociationDefinition.LabelData(null, null, labelField, from), target, null, schema, "true", null, null, null, null, null, false, null, null);
        }


        public static ApplicationAssociationDefinition GetInstance([NotNull] string @from, [NotNull]ApplicationAssociationDefinition.LabelData labelData, string target, string qualifier, ApplicationAssociationSchemaDefinition applicationAssociationSchema,
                                                                   string showExpression, string helpIcon, string toolTip, string requiredExpression, ISet<ApplicationEvent> events, string defaultValue, bool hideDescription,
            string orderbyfield, string defaultExpression, string extraProjectionFields = null, string isEnabled = "true", bool forceDistinctOptions = true, string valueField = null, ApplicationSection detailSection = null) {

            var association = new ApplicationAssociationDefinition(from, labelData, target, qualifier, applicationAssociationSchema, showExpression, helpIcon,
                                                                   toolTip, requiredExpression, defaultValue, hideDescription, orderbyfield,
                                                                   defaultExpression, isEnabled, events, forceDistinctOptions,
                                                                   valueField, detailSection);

            var labelField = labelData.LabelField;
            association.LabelFields = ParseLabelFields(labelField);
            association.ApplicationTo = ParseApplicationTo(labelField);
            association.OriginalLabelField = labelField;
            if (extraProjectionFields != null) {
                BuildExtraProjectionFields(association, extraProjectionFields);
            }
            association.SetLazyResolver(new Lazy<EntityAssociation>(
                () => {
                    var appMetadata = MetadataProvider.Application(association.From);
                    return MetadataProvider.Entity(appMetadata.Entity).LocateAssociationByLabelField(labelField).Item1;
                }));
            association.SetLazyRendererParametersResolver(new Lazy<IDictionary<string, object>>(() => {
                var metadataParameters = association.InnerRendererParameters;
                var result = new Dictionary<string, object>();
                if (!association.RendererType.EqualsIc("modal")) {
                    return result;
                }
                string appName;
                metadataParameters.TryGetValueAsString("application", out appName);
                string schemaName;
                metadataParameters.TryGetValueAsString("schema", out schemaName);
                var app = MetadataProvider.Application(appName);
                var schema = app.Schema(new ApplicationMetadataSchemaKey(schemaName));
                result.Add("schema", schema);
                return result;
            }));
            MergeWithStereotypeComponent(association);
            return association;
        }


        private static void BuildExtraProjectionFields(ApplicationAssociationDefinition association, string extraProjectionFields) {
            string[] collection = extraProjectionFields.Split(',');
            var relationshipName = EntityUtil.GetRelationshipName(association.ApplicationTo);
            foreach (var s in collection) {
                association.ExtraProjectionFields.Add(s.Trim());
            }

        }

        private static string ParseApplicationTo(string labelField) {
            var indexOf = labelField.IndexOf(".", System.StringComparison.InvariantCulture);
            var firstAttribute = labelField.Substring(0, indexOf);
            return EntityUtil.GetRelationshipName(firstAttribute);
        }

        //may be passed as a comma separeted list : entity.field1,entity.field2 == > [field1, field2]
        private static IList<string> ParseLabelFields(string labelField) {
            IList<string> resultingLabels = new List<string>();
            var labelFields = labelField.Split(',');
            foreach (var field in labelFields) {
                var idx = field.IndexOf(".", System.StringComparison.Ordinal);
                if (idx == -1) continue;
                resultingLabels.Add(field.Substring(idx + 1));
            }

            return resultingLabels;
        }

        //        protected static EntityAssociation LookupEntityAssociation(ApplicationAssociationDefinition association) {
        //            var appMetadata = MetadataProvider.Application(association.From);
        //            var indexOf = association._labelField.IndexOf(".", StringComparison.Ordinal);
        //            var firstPart = _labelField.Substring(0, indexOf);
        //            var lookupString = firstPart.EndsWith("_") ? firstPart : firstPart + "_";
        //            return MetadataProvider.Entity(appMetadata.Entity).Associations.FirstOrDefault(a => a.Qualifier == lookupString);
        //        }
        public static ApplicationAssociationSchemaDefinition GetSchemaInstance(AssociationDataProvider dataProvider, AssociationFieldRenderer renderer,
            FieldFilter filter, string dependantFieldsST) {
            var schema = new ApplicationAssociationSchemaDefinition(dataProvider, renderer, filter);
            if (schema.DataProvider != null) {
                schema.DependantFields = DependencyBuilder.TryParsingDependentFields(schema.DataProvider.WhereClause);
            }
            if (dependantFieldsST != null) {
                var fields = dependantFieldsST.Split(',');
                foreach (var field in fields) {
                    schema.DependantFields.Add(field);
                }
            }
            return schema;
        }

        private static void MergeWithStereotypeComponent(ApplicationAssociationDefinition association) {
            var stereotypeProvider = ComponentStereotypeFactory.LookupStereotype(association.RendererStereotype);
            if (stereotypeProvider == null) {
                return;
            }
            var stereotypeProperties = stereotypeProvider.StereotypeProperties();

            foreach (var stereotypeProperty in stereotypeProperties) {
                string key = stereotypeProperty.Key;
                if (!association.RendererParameters.ContainsKey(key)) {
                    association.RendererParameters.Add(key, stereotypeProperty.Value);
                }
            }
        }
    }
}

