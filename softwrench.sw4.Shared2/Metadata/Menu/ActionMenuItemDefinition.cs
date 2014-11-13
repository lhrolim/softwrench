using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softwrench.sW4.Shared2.Metadata.Menu {

    public class ActionMenuItemDefinition : MenuBaseDefinition, IMenuLeaf {

        public string Action { get; set; }
        public string Controller { get; set; }
        public string Target { get; set; }
        public IDictionary<string, string> Parameters { get; set; }

        public ActionMenuItemDefinition() {

        }

        public ActionMenuItemDefinition(string id, string title, string role, string tooltip, string icon,
            string action, string controller, string target, IDictionary<string, string> parameters, string moduleName)
            : base(id, title, role, tooltip, icon) {
            Action = action;
            Controller = controller;
            Parameters = parameters;
            Target = target;
            Module = moduleName;
        }
    }
}
