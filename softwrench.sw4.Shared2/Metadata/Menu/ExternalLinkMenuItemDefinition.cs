using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softwrench.sW4.Shared2.Metadata.Menu {

    /// <summary>
    /// Used to link the application to an external link, opening it on a new browser window
    /// </summary>
    public class ExternalLinkMenuItemDefinition : MenuBaseDefinition, IMenuLeaf {

        public string Link { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public string ModuleName { get; set; }

        public ExternalLinkMenuItemDefinition() {

        }

        public ExternalLinkMenuItemDefinition(string id, string title, string role, string tooltip, string icon,
            string link, IDictionary<string, object> parameters, string moduleName,string permissionExpression, string customizationPosition)
            : base(id, title, role, tooltip, icon, customizationPosition) {
            Link = link;
            PermissionExpresion = permissionExpression;
            Parameters = parameters;
            ModuleName = moduleName;
        }
    }
}
