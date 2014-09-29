using System.Collections.Generic;
using softwrench.sW4.Shared.Metadata.Menu.Interfaces;

namespace softwrench.sW4.Shared.Metadata.Menu.Containers {
    public class MenuContainerDefinition : MenuBaseDefinition {
        public IEnumerable<MenuBaseDefinition> Leafs { get; set; }

        public MenuContainerDefinition() { }

        public MenuContainerDefinition(string id, string title, string role, string tooltip, string icon, IEnumerable<MenuBaseDefinition> leafs)
            : base(id, title, role, tooltip, icon) {
            Leafs = leafs;
        }
    }
}
