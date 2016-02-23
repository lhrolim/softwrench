using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema {
    public abstract class BaseApplicationFieldDefinition : BaseDefinition, IApplicationAttributeDisplayable, IDefaultValueApplicationDisplayable {

        public string ApplicationName { get; set; }
        [DefaultValue("")] public string Label { get; set; }
        public string Attribute { get; set; }
        [DefaultValue("false" )] public string RequiredExpression { get; set; }
        public bool IsReadOnly { get; set; }
        public string DefaultValue { get; set; }
        public string Qualifier { get; set; }
        public abstract bool IsHidden { get; set; }

        [JsonIgnore]
        public bool DeclaredAsQueryOnEntity { get; set; }
        [DefaultValue("true")] public string ShowExpression { get; set; }
        [DefaultValue("true")] public string EnableExpression { get; set; }

        public string ToolTip { get; set; }
        public bool? ReadOnly { get; set; }
        public string AttributeToServer { get; set; }

        public string DefaultExpression { get; set; }
        private IDictionary<String, ApplicationEvent> _events = new Dictionary<string, ApplicationEvent>();

        public abstract string RendererType { get; }
        public string Type { get { return GetType().Name; } }
        public string Role {
            get { return Attribute; }
        }

        public BaseApplicationFieldDefinition() {

        }

        protected BaseApplicationFieldDefinition(string applicationName, string label,
            string attribute, string requiredExpression, bool isReadOnly,
            string defaultValue, string qualifier, string showExpression, string toolTip,
            string attributeToServer, ISet<ApplicationEvent> events, string enableExpression,
            string defaultExpression, bool declaredAsQueryOnEntity) {
            if (attribute == null) {
                throw new ArgumentNullException("attribute", String.Format("check {0} metadata config", applicationName));
            }

            DefaultExpression = defaultExpression;
            ApplicationName = applicationName;
            Label = label;
            Attribute = attribute;
            RequiredExpression = requiredExpression;
            IsReadOnly = isReadOnly;
            DefaultValue = defaultValue;
            Qualifier = qualifier;
            ShowExpression = showExpression;
            ToolTip = toolTip;
            AttributeToServer = attributeToServer;
            if (events != null) {
                _events = events.ToDictionary(f => f.Type, f => f);
            }
            EnableExpression = enableExpression;
            DeclaredAsQueryOnEntity = declaredAsQueryOnEntity;
            }

        public IDictionary<String, ApplicationEvent> Events {
            get { return _events; }
            set { _events = value; }
        }

        protected bool Equals(BaseApplicationFieldDefinition other) {
            return string.Equals(Attribute, other.Attribute);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseApplicationFieldDefinition)obj);
        }

        public override int GetHashCode() {
            return (Attribute != null ? Attribute.GetHashCode() : 0);
        }
    }
}
