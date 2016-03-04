using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sw4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Applications.UI {
    public class ApplicationTabDefinition : IApplicationIndentifiedDisplayable, IApplicationIdentifier, IApplicationDisplayableContainer {

        public String Id {
            get; set;
        }
        public String ApplicationName {
            get; set;
        }
        public string Label {
            get; set;
        }
        public string Icon {
            get; set;
        }
        private List<IApplicationDisplayable> _displayables = new List<IApplicationDisplayable>();

        private string _role;

        public ApplicationTabDefinition(string id, string applicationName, string label, List<IApplicationDisplayable> displayables, string toolTip,
            string showExpression, string icon, string role) {
            Id = id;
            ApplicationName = applicationName;
            Label = label;
            ToolTip = toolTip;
            ShowExpression = showExpression;
            _displayables = displayables;
            Icon = icon;
            _role = role;
        }


        public string RendererType {
            get {
                return null;
            }
        }
        public string Type {
            get {
                return typeof(ApplicationTabDefinition).Name;
            }
        }

        public string Role {
            get {
                if (_role != null) {
                    return _role;
                }
                return ApplicationName + "." + Id;
            }
            set {
                _role = value;
            }
        }

        public string ShowExpression {
            get; set;
        }
        public string ToolTip {
            get; set;
        }
        public string IdFieldName {
            get; set;
        }
        public bool IsReadOnly {
            get {
                return false;
            }
            set {
            }
        }
        public string TabId {
            get {
                return Id;
            }
        }

        public List<IApplicationDisplayable> Displayables {
            get {
                return _displayables;
            }
            set {
                _displayables = value;
            }
        }

        public string Attribute {
            get {
                return TabId;
            }
            set {
            }
        }
    }
}
