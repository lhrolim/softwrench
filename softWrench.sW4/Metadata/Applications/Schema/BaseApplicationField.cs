using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace softWrench.sW4.Metadata.Applications.Schema {


    public abstract class BaseApplicationField : IApplicationAttributeDisplayable, IDefaultValueApplicationDisplayable {

        protected readonly string _applicationName;
        protected readonly string _label;
        protected readonly string _attribute;
        protected readonly bool _isRequired;
        protected readonly bool _isReadOnly;
        protected readonly string _defaultValue;
        protected readonly string _qualifier;
        protected readonly string _showExpression;
        protected readonly string _toolTip;

        protected BaseApplicationField(string applicationName, string label,
            string attribute, bool isRequired, bool isReadOnly,
            string defaultValue, string qualifier, string showExpression, string toolTip) {
            if (attribute == null) throw new ArgumentNullException("attribute");
            if (label == null) throw new ArgumentNullException("label");
            _applicationName = applicationName;
            _label = label;
            _attribute = attribute;
            _isRequired = isRequired;
            _isReadOnly = isReadOnly;
            _defaultValue = defaultValue;
            _qualifier = qualifier;
            _showExpression = showExpression;
            _toolTip = toolTip;
            }

        [JsonIgnore]
        public string ApplicationName {
            get { return _applicationName; }
        }

        [NotNull]
        public string Label {
            get { return _label; }
        }

        [NotNull]
        public string Attribute {
            get { return _attribute; }
        }

        public bool IsRequired {
            get { return _isRequired; }
        }

        public bool IsReadOnly {
            get { return _isReadOnly; }
        }

        public string Qualifier {
            get { return _qualifier; }
        }


        public abstract string RendererType { get; }
        public string Type { get { return GetType().Name; } }
        public string DefaultValue { get { return _defaultValue; } }

        public string ShowExpression {get { return _showExpression; }
        }

        public string ToolTip { get { return _toolTip; } }

        public string Role {
            get { return MetadataProvider.Application(_applicationName).Role + "." + Attribute; }
        }
    }
}
