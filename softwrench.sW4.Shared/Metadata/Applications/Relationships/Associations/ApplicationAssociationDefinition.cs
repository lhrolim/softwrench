using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Metadata.Applications.Schema;
using softwrench.sW4.Shared.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared.Metadata.Entity.Association;
using softwrench.sW4.Shared.Util;

namespace softwrench.sW4.Shared.Metadata.Applications.Relationships.Associations {

    public class ApplicationAssociationDefinition : ApplicationRelationshipDefinition, IDataProviderContainer, IDefaultValueApplicationDisplayable {

        private readonly string _label;
        protected readonly string _labelField;
        private readonly string _labelPattern;
        private readonly string _target;
        private readonly string _defaultValue;
        private string _applicationTo;
        private readonly ISet<string> _extraProjectionFields = new HashSet<string>();




        private readonly ApplicationAssociationSchemaDefinition _applicationAssociationSchema;

        private IList<String> _labelFields = new List<string>();

        public class LabelData {

            public string Label { get; set; }
            public string LabelPattern { get; set; }
            public string LabelField { get; set; }

            public LabelData(string label, string labelPattern, string labelField, string applicationName) {
                Label = label;
                LabelPattern = labelPattern;
                LabelField = labelField;
                if (labelPattern != null) {
                    string[] fields = LabelField.Split(',');
                    try {
                        var a = String.Format(LabelPattern, fields);
                    } catch (Exception e) {
                        throw new InvalidOperationException(String.Format("incompatible labelPattern and Label Fields at application {0}", applicationName));
                    }
                }
            }


        }


        public ApplicationAssociationDefinition(string @from, LabelData labelData, string target, ApplicationAssociationSchemaDefinition applicationAssociationSchema,
            string showExpression, string toolTip, string defaultValue = null, string extraProjectionFields = null)
            : base(from, labelData.Label, showExpression, toolTip) {

            _label = labelData.Label;
            _labelField = labelData.LabelField;
            _labelPattern = labelData.LabelPattern;
            _target = target;
            _applicationAssociationSchema = applicationAssociationSchema;
            _defaultValue = defaultValue;
          
        }

      

        private string ParseApplicationTo(string labelField) {
            var indexOf = labelField.IndexOf(".", System.StringComparison.InvariantCulture);
            var firstAttribute = labelField.Substring(0, indexOf);
            return EntityUtil.GetRelationshipName(firstAttribute);
        }

        //may be passed as a comma separeted list : entity.field1,entity.field2 == > [field1, field2]
        private IList<string> ParseLabelFields(string labelField) {
            IList<string> resultingLabels = new List<string>();
            var labelFields = labelField.Split(',');
            foreach (var field in labelFields) {
                var idx = field.IndexOf(".", System.StringComparison.Ordinal);
                if (idx == -1) continue;
                resultingLabels.Add(field.Substring(idx + 1));
            }

            return resultingLabels;
        }

        protected override EntityAssociation LookupEntityAssociation() {
            //            var appMetadata = MetadataProvider.Application(From);
            //            var indexOf = _labelField.IndexOf(".", StringComparison.Ordinal);
            //            var firstPart = _labelField.Substring(0, indexOf);
            //            var lookupString = firstPart.EndsWith("_") ? firstPart : firstPart + "_";
            //            return MetadataProvider.Entity(appMetadata.Entity).Associations.FirstOrDefault(a => a.Qualifier == lookupString);
            return null;
        }

        public IEnumerable<EntityAssociationAttribute> LookupAttributes() {
            if (EntityAssociation == null) {
                return null;
            }
            var entityAssociationAttributes = EntityAssociation.Attributes;
            return entityAssociationAttributes.Where(attribute => !attribute.Primary).ToList();
        }

        public ApplicationAssociationSchemaDefinition Schema {
            get { return _applicationAssociationSchema; }
        }

        public string Target {
            get { return _target; }
        }

        public string DefaultValue {
            get { return _defaultValue; }
        }

        public string Attribute { get { return Target; } }

        public IList<string> LabelFields {
            get { return _labelFields; }
            set { _labelFields = value; }
        }

        public string ApplicationTo {
            get { return _applicationTo; }
            set { _applicationTo = value; }
        }

        public string LabelPattern {
            get { return _labelPattern; }
        }

        public string ApplicationPath {
            get { return From + "." + _applicationTo; }
        }

        public Boolean MultiValued {
            get { return ExtraProjectionFields.Count > 0; }
        }

        public override string ToString() {
            return string.Format("From: {0}, To: {1} , Target: {2}", From, EntityAssociation, Target);
        }

        public override string RendererType {
            get { return _applicationAssociationSchema.Renderer.RendererType.ToLower(); }
        }

        public ISet<string> ExtraProjectionFields {
            get { return _extraProjectionFields; }
        }

        /// <summary>
        /// Return whether this association should be resolved on a lazy fashion, meaning that the data will only be fetched on a later phasis.
        ///  This decision will be delegated to the renderer type; a combo is a good example of a eager association, whileas a modal and autocomplete are lazy fetched components. 
        /// Those components data will only be fetched when the user makes some kind of interaction with it.
        /// </summary>
        public Boolean IsLazyLoaded {
            get { return _applicationAssociationSchema.IsLazyLoaded; }
        }

        public ISet<string> DependantFields {
            get { return _applicationAssociationSchema.DependantFields; }
        }

        public string AssociationKey { get { return _applicationTo; } }

        public override string Role { get { return From + "." + Target; } }
    }
}
