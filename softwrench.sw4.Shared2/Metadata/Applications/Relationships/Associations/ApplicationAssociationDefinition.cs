﻿using System;
using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softwrench.sW4.Shared2.Util;

namespace softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations {

    public class ApplicationAssociationDefinition :
        ApplicationRelationshipDefinition, IDataProviderContainer, IDefaultValueApplicationDisplayable, IDependableField, IApplicationAttributeDisplayable, IPCLCloneable {

        private string _label;
        protected string LabelField;
        public string DefaultValue { get; set; }
        public string Target { get; set; }
        public string LabelPattern { get; set; }
        public string EnableExpression { get; set; }

        private string _applicationTo;
        private ISet<string> _extraProjectionFields = new HashSet<string>();

        private IDictionary<String, ApplicationEvent> _events = new Dictionary<string, ApplicationEvent>();

        public string RequiredExpression { get; set; }

        public string Qualifier { get; set; }

        private ApplicationAssociationSchemaDefinition _applicationAssociationSchema;

        private IList<String> _labelFields = new List<string>();
        private LabelData _labelData;
        private ISet<ApplicationEvent> _eventsSet;

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

        public ApplicationAssociationDefinition() { }

        public ApplicationAssociationDefinition(string @from, LabelData labelData, string target, string qualifier, ApplicationAssociationSchemaDefinition applicationAssociationSchema,
            string showExpression, string toolTip, string requiredExpression, string defaultValue, string enableExpression = "true", ISet<ApplicationEvent> events = null)
            : base(from, labelData.Label, showExpression, toolTip) {
            _labelData = labelData;
            _label = labelData.Label;
            LabelField = labelData.LabelField;
            LabelPattern = labelData.LabelPattern;
            Target = target;
            _applicationAssociationSchema = applicationAssociationSchema;
            DefaultValue = defaultValue;
            EnableExpression = enableExpression;
            RequiredExpression = requiredExpression;
            _eventsSet = events;
            Qualifier = qualifier;

            if (events != null) {
                _events = events.ToDictionary(f => f.Type, f => f);
            }

        }



        private string ParseApplicationTo(string labelField) {
            var indexOf = labelField.IndexOf(".", System.StringComparison.Ordinal);
            var firstAttribute = labelField.Substring(0, indexOf);
            return EntityUtil.GetRelationshipName(firstAttribute);
        }

        //may be passed as a comma separeted list : entity.field1,entity.field2 == > [field1, field2]
        private IList<string> ParseLabelFields(string labelField) {
            IList<string> resultingLabels = new List<string>();
            var labelFields = labelField.Split(',');
            foreach (var field in labelFields) {
                var idx = field.IndexOf(".", System.StringComparison.Ordinal);
                if (idx == -1) {
                    continue;
                }
                resultingLabels.Add(field.Substring(idx + 1));
            }

            return resultingLabels;
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
            set { _applicationAssociationSchema = value; }
        }




        public string Attribute { get { return Target; } set { Target = value; } }

        public IList<string> LabelFields {
            get { return _labelFields; }
            set { _labelFields = value; }
        }

        public string ApplicationTo {
            get { return _applicationTo; }
            set { _applicationTo = value; }
        }

        public IDictionary<string, ApplicationEvent> Events {
            get { return _events; }
            set { _events = value; }
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
            get { return base.RendererType ?? _applicationAssociationSchema.Renderer.RendererType.ToLower(); }        
        }

        public override IDictionary<string, string> RendererParameters {
            get { return _applicationAssociationSchema.Renderer == null ? new Dictionary<string, string>() : _applicationAssociationSchema.Renderer.ParametersAsDictionary(); }
        }

        public ISet<string> ExtraProjectionFields {
            get { return _extraProjectionFields; }
            set { _extraProjectionFields = value; }
        }

        /// <summary>
        /// Return whether this association should be resolved on a lazy fashion, meaning that the data will only be fetched on a later phasis.
        ///  This decision will be delegated to the renderer type; a combo is a good example of a eager association, whileas a modal and autocomplete are lazy fetched components. 
        /// Those components data will only be fetched when the user makes some kind of interaction with it.
        /// </summary>
        public Boolean IsLazyLoaded() {
            return _applicationAssociationSchema.IsLazyLoaded;
        }

        public Boolean IsPaginated() {
            return _applicationAssociationSchema.IsPaginated;
        }

        public ISet<string> DependantFields {
            get { return _applicationAssociationSchema.DependantFields; }
        }

        public string AssociationKey { get { return _applicationTo; } }

        public override string Role { get { return From + "." + Target; } }

        public object Clone() {
            var cloned = new ApplicationAssociationDefinition(From, _labelData, Target,Qualifier, Schema, ShowExpression, ToolTip, RequiredExpression,
                DefaultValue, EnableExpression, _eventsSet) {
                    ExtraProjectionFields = ExtraProjectionFields,
                    LabelFields = LabelFields,
                    ApplicationTo = ApplicationTo,
                };
            cloned.SetLazyResolver(LazyEntityAssociation);
            return cloned;
        }
    }
}
