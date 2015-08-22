using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Metadata.Security;
using softwrench.sw4.Shared2.Metadata.Applications.Command;

namespace softWrench.sW4.Metadata.Applications.Command {
    static class ApplicationCommandExtensions {
        public static ContainerCommand Secure(this ContainerCommand container, InMemoryUser user) {
            var secureLeafs = new List<ICommandDisplayable>();
            foreach (var leaf in container.Displayables) {
                if (!user.IsSwAdmin() && leaf.Role != null && (user.Roles == null || !user.Roles.Any(r => r.Active && r.Name == leaf.Role))) {
                    continue;
                }
                if (leaf is ContainerCommand) {
                    var secured = ((ContainerCommand)leaf).Secure(user);
                    if (secured != null) {
                        secureLeafs.Add(secured);
                    }
                } else {
                    secureLeafs.Add(leaf);
                }
            }
            return !secureLeafs.Any() ? null : new ContainerCommand(container.Id, container.Label, container.Tooltip, container.Role, container.Position,container.Icon,container.Service,container.Method, secureLeafs);
        }
    }
}
