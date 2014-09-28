using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softwrench.sw4.Shared2.Metadata.Modules;

namespace softwrench.sW4.Shared2.Metadata.Menu.Containers {

    public class MenuDefinition {
        public enum MenuDisplacement {
            Vertical, Horizontal,
        }

        public MenuDefinition() { }

        public MenuDefinition(IEnumerable<MenuBaseDefinition> leafs, MenuDisplacement mainMenuDisplacement, IMenuLeaf indexItem) {
            MainMenuDisplacement = mainMenuDisplacement;
            Leafs = leafs;
            IndexItem = indexItem;
        }

        public MenuDefinition(IEnumerable<MenuBaseDefinition> leafs, String mainMenuDisplacement, IMenuLeaf indexItem) {
            MainMenuDisplacement = (MenuDisplacement) Enum.Parse(typeof(MenuDisplacement),mainMenuDisplacement,true);
            Leafs = leafs;
            IndexItem = indexItem;
        }

        public MenuDisplacement MainMenuDisplacement { get; set; }
        public string Displacement {
            get {
                return MainMenuDisplacement.ToString().ToLower();
            }
        }
        public IMenuLeaf IndexItem { get; set; }
        public IEnumerable<MenuBaseDefinition> Leafs { get; set; }

        public List<ModuleDefinition> Modules { get; set; }  

        public override string ToString() {
            return string.Format("Leafs: {0}, MainMenuDisplacement: {1}", Leafs, MainMenuDisplacement);
        }
    }
}
