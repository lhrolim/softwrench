using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sW4.Shared.Metadata.Applications.Schema {
    public abstract class BaseApplicationFieldDefinition : IApplicationAttributeDisplayable, IDefaultValueApplicationDisplayable {

        public string ApplicationName { get; set; }
        public string Label { get; set; }
        public string Attribute { get; set; }
        public bool IsRequired { get; set; }
        public bool IsReadOnly { get; set; }
        public string DefaultValue { get; set; }
        public string Qualifier { get; set; }
        public string ShowExpression { get; set; }
        public string ToolTip { get; set; }

        public abstract string RendererType { get; }
        public string Type { get { return GetType().Name; } }
        public string Role {
            get { return ApplicationName + "." + Attribute; }
        }

        public BaseApplicationFieldDefinition() {

        }

        protected BaseApplicationFieldDefinition(string applicationName, string label,
            string attribute, bool isRequired, bool isReadOnly,
            string defaultValue, string qualifier, string showExpression, string toolTip) {
            if (attribute == null) throw new ArgumentNullException("attribute");
            if (label == null) throw new ArgumentNullException("label");
            ApplicationName = applicationName;
            Label = label;
            Attribute = attribute;
            IsRequired = isRequired;
            IsReadOnly = isReadOnly;
            DefaultValue = defaultValue;
            Qualifier = qualifier;
            ShowExpression = showExpression;
            ToolTip = toolTip;
        }

        //        public string Role {
        //            get { return MetadataProvider.Application(_applicationName).Role + "." + Attribute; }
        //        }
    }
}
