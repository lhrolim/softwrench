using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Applications.UI {
    public class ApplicationTabDefinition : IApplicationDisplayable, IApplicationIdentifier, IApplicationDisplayableContainer {

        public String Id { get; set; }
        public String ApplicationName { get; set; }
        public string Label { get; set; }
        private List<IApplicationDisplayable> _displayables = new List<IApplicationDisplayable>();

        public ApplicationTabDefinition(string id, string applicationName, string label, List<IApplicationDisplayable> displayables, string toolTip,
            string showExpression) {
            Id = id;
            ApplicationName = applicationName;
            Label = label;
            ToolTip = toolTip;
            ShowExpression = showExpression;
            _displayables = displayables;
        }


        public string RendererType { get { return null; } }
        public IDictionary<string, string> RendererParameters { get { return new Dictionary<string, string>(); } }
        public string Type { get { return typeof(ApplicationTabDefinition).Name; } }
        public string Role { get { return ApplicationName + "." + Id; } }
        public string ShowExpression { get; set; }
        public string EnableExpression { get; set; }
        public string ToolTip { get; set; }
        public string IdFieldName { get; set; }
        public bool? ReadOnly { get { return false; } set { } }
        public string TabId { get { return Id; } }

        public List<IApplicationDisplayable> Displayables {
            get { return _displayables; }
            set { _displayables = value; }
        }
    }
}
