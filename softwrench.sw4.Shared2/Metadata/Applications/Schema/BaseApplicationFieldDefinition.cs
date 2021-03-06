﻿using softwrench.sw4.Shared2.Metadata;
using softwrench.sw4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace softwrench.sW4.Shared2.Metadata.Applications.Schema {
    public abstract class BaseApplicationFieldDefinition : BaseDefinition, IApplicationAttributeDisplayable, IDefaultValueApplicationDisplayable {

        public string ApplicationName { get; set; }
        public string Label { get; set; }
        public string Attribute { get; set; }
        
        public bool IsReadOnly { get; set; }
        public string RequiredExpression {
            get; set;
        }
        public string DefaultValue { get; set; }
        public string Qualifier { get; set; }

        public string ShowExpression { get; set; }
        public string EnableExpression { get; set; }

        public string ToolTip { get; set; }
        public bool? ReadOnly { get; set; }
        public string AttributeToServer { get; set; }

        private IDictionary<String, ApplicationEvent> _events = new Dictionary<string, ApplicationEvent>();

        public abstract string RendererType { get; }

        public abstract IDictionary<string, string> RendererParameters { get; }

        public string Type { get { return GetType().Name; } }
        public string Role {
            get { return ApplicationName + "." + Attribute; }
        }

        public BaseApplicationFieldDefinition() {

        }

        protected BaseApplicationFieldDefinition(string applicationName, string label,
            string attribute, string requiredExpression, bool isReadOnly,
            string defaultValue, string qualifier, string showExpression, string toolTip,
            string attributeToServer, ISet<ApplicationEvent> events, string enableExpression) {
            if (attribute == null) {
                throw new ArgumentNullException("attribute", String.Format("check {0} metadata config", applicationName));
            }

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
