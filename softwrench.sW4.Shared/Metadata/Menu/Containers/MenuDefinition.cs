using System;
using System.Collections.Generic;
using System.Linq;
using softwrench.sW4.Shared.Metadata.Menu.Interfaces;

namespace softwrench.sW4.Shared.Metadata.Menu.Containers {

    public class MenuDefinition {
        public enum MenuDisplacement {
            Vertical, Horizontal,
        }

        public MenuDisplacement MainMenuDisplacement { get; set; }
        public IMenuLeaf IndexItem { get; set; }
        public IEnumerable<MenuBaseDefinition> Leafs { get; set; }
      
        public override string ToString() {
            return string.Format("Leafs: {0}, MainMenuDisplacement: {1}", Leafs, MainMenuDisplacement);
        }
    }
}
