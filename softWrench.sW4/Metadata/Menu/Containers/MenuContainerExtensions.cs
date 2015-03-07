using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softWrench.sW4.Metadata.Menu.Containers {
    public static class MenuContainerExtensions {
        public static MenuBaseDefinition Secure(this MenuContainerDefinition container, InMemoryUser user) {
            var secureLeafs = new List<MenuBaseDefinition>();
            foreach (var leaf in container.Leafs) {
                if (!user.IsSwAdmin() && leaf.Role != null && (user.Roles == null || !user.Roles.Any(r => r.Active && r.Name == leaf.Role))) {
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
                container.Title, container.Role, container.Tooltip, container.Icon, container.Module,container.Controller,container.Action,container.HasMainAction, secureLeafs);
        }
    }
}
