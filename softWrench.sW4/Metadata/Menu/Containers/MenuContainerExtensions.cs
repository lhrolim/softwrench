using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Menu;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softWrench.sW4.Metadata.Menu.Containers {
    public static class MenuContainerExtensions {


        public static bool IsRestrictedByRole(this MenuBaseDefinition leaf, InMemoryUser user) {
            var roles = user.Roles;
            string applicationBasedRole = null;
            if (leaf is MenuContainerDefinition) {
                var applicationRef = ((MenuContainerDefinition)leaf).ApplicationContainer;
                if (MetadataProvider.ApplicationRoleAlias.ContainsKey(applicationRef)) {
                    //this means for instance, that a menu protecting a workorder application could be activated by a role name called workorder, regardless of the the name of the role property of the menu itself
                    applicationBasedRole = applicationRef;
                }
            } else if (leaf.RoleDefinedByParent) {
                applicationBasedRole = ((ApplicationMenuItemDefinition)leaf).Application;
            }
            return !roles.Any(r => r.Active && (r.Name.EqualsIc(leaf.Role) || (applicationBasedRole != null && r.Name.EqualsIc(applicationBasedRole))));
        }


        public static MenuBaseDefinition Secure(this MenuContainerDefinition container, InMemoryUser user) {
            var secureLeafs = new List<MenuBaseDefinition>();
            foreach (var leaf in container.Leafs) {
                if (!user.IsSwAdmin() && leaf.Role != null && (user.Roles == null || IsRestrictedByRole(leaf, user))) {
                    continue;
                }
                if (leaf is MenuContainerDefinition) {
                    var secured = ((MenuContainerDefinition)leaf).Secure(user);
                    if (secured != null) {
                        secureLeafs.Add(secured);
                    }
                } else {
                    secureLeafs.Add(leaf);
                }
            }
            return !secureLeafs.Any() ? null : new MenuContainerDefinition(container.Id,
                container.Title, container.Role, container.Tooltip, container.Icon, container.Module, container.Controller, container.Action, container.HasMainAction, secureLeafs);
        }
    }
}
