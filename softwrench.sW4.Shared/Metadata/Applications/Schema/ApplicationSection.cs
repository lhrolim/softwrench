using System.Collections.Generic;
using softwrench.sW4.Shared.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.Shared.Util;

namespace softwrench.sW4.Shared.Metadata.Applications.Schema {
    public class ApplicationSection : IApplicationAttributeDisplayable {

        private readonly string _id;
        private readonly string _applicationName;
        private readonly bool _abstract;
        private readonly string _resourcepath;
        private readonly string _label;
        private readonly string _attribute;
        private readonly IDictionary<string, string> _parameters;
        private readonly string _showExpression;
        private readonly string _toolTip;
        private readonly IList<IApplicationDisplayable> _displayables = new List<IApplicationDisplayable>();

        public ApplicationSection(string id, string applicationName, 
            bool @abstract, string label, string attribute, string resourcepath, 
            string parameters, IList<IApplicationDisplayable> displayables, string showExpression, string toolTip) {
            _id = id;
            _applicationName = applicationName;
            _abstract = @abstract;
            _resourcepath = resourcepath;
            _parameters = PropertyUtil.ConvertToDictionary(parameters);
            _displayables = displayables;
            _label = label;
            _attribute = attribute;
            _showExpression = showExpression;
            _toolTip = toolTip;
        }

        public string RendererType { get { return null; } }
        public string Type { get { return GetType().Name; } }


        public string Id {
            get { return _id; }
        }

        public IList<IApplicationDisplayable> Displayables {
            get { return _displayables; }
        }

        public bool Abstract {
            get { return _abstract; }
        }

        public string Resourcepath {
            get { return _resourcepath; }
        }

        public string Label {
            get { return _label; }
        }

        public IDictionary<string, string> Parameters {
            get { return _parameters; }
        }

        public string Attribute {
            get { return _attribute; }
        }

        public string Role {
            get { return _applicationName + "." + Id; }
        }

        public string ShowExpression {
            get { return _showExpression; }
        }

        public string ToolTip {
            get { return _toolTip; }
        }

        public override string ToString() {
            return string.Format("Id: {0}, Displayables: {1}, Abstract: {2}", _id, _displayables.Count, _abstract);
        }
    }
}
