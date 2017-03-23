using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softwrench.sW4.Shared2.Metadata.Menu {

    /// <summary>
    /// Used to link the application to an external link, opening it on a new browser window
    /// </summary>
    public class ReferenceMenuItemDefinition : MenuContainerDefinition, IMenuLeaf {

        public ReferenceMenuItemDefinition(string id, string title, string role, string tooltip, string icon,
            string module, string controller, string action, bool hasMainAction,string permissionExpression, IDictionary<string, object> parameters, IEnumerable<MenuBaseDefinition> leafs)
            : base(id, title, role, tooltip, icon, module, controller, action, hasMainAction,null, permissionExpression, parameters, leafs) {

        }
    }
}
