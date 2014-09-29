using System.Collections.Generic;
using softwrench.sW4.Shared.Metadata.Menu.Interfaces;

namespace softwrench.sW4.Shared.Metadata.Menu {

    public class ActionMenuItemDefinition : MenuBaseDefinition, IMenuLeaf {

        public string Action { get; set; }
        public string Controller { get; set; }
        public IDictionary<string, string> Parameters { get; set; }

        public ActionMenuItemDefinition() {

        }

        public ActionMenuItemDefinition(string id, string title, string role, string tooltip, string icon, string action, string controller, IDictionary<string, string> parameters)
            : base(id, title, role, tooltip, icon) {
            Action = action;
            Controller = controller;
            Parameters = parameters;
        }
    }
}
