using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softwrench.sW4.Shared2.Metadata.Menu {

    public class ActionMenuItemDefinition : MenuBaseDefinition, IMenuLeaf, IMenuAction {

        public string Action { get; set; }
        public string Controller { get; set; }
        public string Target { get; set; }        
        public IDictionary<string, object> Parameters { get; set; }
        public string ModuleName { get; set; }

        public ActionMenuItemDefinition() {

        }

        public ActionMenuItemDefinition(string id, string title, string role, string tooltip, string icon, 
            string action, string controller, string target, IDictionary<string, object> parameters,string moduleName,string permissionExpression,string customizationPosition)
            : base(id, title, role, tooltip, icon, customizationPosition) {
            Action = action;
            PermissionExpresion = permissionExpression;
            Controller = controller;
            Parameters = parameters;
            ModuleName = moduleName;
            Target = target;
        }
    }
}
