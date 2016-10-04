using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Util;
using softwrench.sw4.Shared2.Metadata.Applications.UI;

namespace softWrench.sW4.Metadata.Applications.Association {
    public class ApplicationAssociationFactory {

        public static ApplicationAssociationDefinition GetInstance([NotNull] string @from, ApplicationAssociationDefinition.LabelData labelData, string target,string qualifier, ApplicationAssociationSchemaDefinition applicationAssociationSchema, 
            string showExpression, string toolTip, string required, ISet<ApplicationEvent> events, string defaultValue, string extraProjectionFields = null, string isEnabled = "true") {
            var association = new ApplicationAssociationDefinition(from, labelData, target,qualifier, applicationAssociationSchema, showExpression,
                toolTip, required, defaultValue, isEnabled, events);
            var labelField = labelData.LabelField;
            association.LabelFields = ParseLabelFields(labelField);
            association.ApplicationTo = ParseApplicationTo(labelField);
            if (extraProjectionFields != null) {
                BuildExtraProjectionFields(association, extraProjectionFields);
            }
            association.SetLazyResolver(new Lazy<EntityAssociation>(
                () => {
                    var appMetadata = MetadataProvider.Application(association.From);
                    var indexOf = labelField.IndexOf(".", StringComparison.Ordinal);
                    var firstPart = labelField.Substring(0, indexOf);
                    var lookupString = firstPart.EndsWith("_") ? firstPart : firstPart + "_";
                    if (Char.IsNumber(lookupString[0])) {
                        lookupString = lookupString.Substring(1);
                    }
                    var entityAssociations = MetadataProvider.Entity(appMetadata.Entity).Associations;
                    return entityAssociations.FirstOrDefault(a => a.Qualifier == lookupString);
                }));
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
    }
}
