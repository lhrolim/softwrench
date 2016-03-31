using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Dataset.Commons;

namespace softWrench.sW4.Metadata.Applications.Command {
    static class ApplicationCommandExtensions {
        public static ContainerCommand Secure(this ContainerCommand container, InMemoryUser user, IList<ActionPermission> permissions) {
            var secureLeafs = new List<ICommandDisplayable>();
            foreach (var leaf in container.Displayables) {
                if (!leaf.Permitted(user, permissions)) {
                    continue;
                }
                if (!user.IsSwAdmin() && leaf.Role != null && (user.Roles == null || !user.Roles.Any(r => r.Active && r.Name == leaf.Role))) {
                    continue;
                }
                if (leaf is ContainerCommand) {
                    var secured = ((ContainerCommand)leaf).Secure(user, permissions);
                    if (secured != null) {
                        secureLeafs.Add(secured);
                    }
                } else {
                    secureLeafs.Add(leaf);
                }
            }
            return !secureLeafs.Any() ? null : new ContainerCommand(container.Id, container.Label, container.Tooltip, container.Role, container.Position, container.Icon, container.Service, container.Method, secureLeafs, container.PermissionExpression);
        }

        public static bool Permitted(this ICommandDisplayable command, InMemoryUser user, IList<ActionPermission> permissions) {

            var rolePermitted = !permissions.Any(p => p.ActionId.EqualsIc(command.Id));
            var externalRolePermitted = string.IsNullOrEmpty(command.Role) || user.IsInRole(command.Role);
            var metadataPermitted = IsMetadataPermitted(command);
            return rolePermitted && externalRolePermitted  && metadataPermitted;
        }

        public static bool IsMetadataPermitted(this ICommandDisplayable command) {
            var expression = command.PermissionExpression;
            if (!string.IsNullOrEmpty(expression)) {
                return GenericSwMethodInvoker.Invoke<bool>(null, expression);
            }
            return true;
        }
    }
}
