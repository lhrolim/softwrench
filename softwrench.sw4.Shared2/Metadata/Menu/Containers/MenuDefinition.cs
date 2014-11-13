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

        public MenuDefinition(IEnumerable<MenuBaseDefinition> leafs, MenuDisplacement mainMenuDisplacement) {
            MainMenuDisplacement = mainMenuDisplacement;
            Leafs = leafs;
        }

        public MenuDefinition(IEnumerable<MenuBaseDefinition> leafs, String mainMenuDisplacement,string itemindexId) {
            MainMenuDisplacement = (MenuDisplacement) Enum.Parse(typeof(MenuDisplacement),mainMenuDisplacement,true);
            Leafs = leafs;
            ItemindexId = itemindexId;
        }

        public string ItemindexId { get; set; }

        public MenuDisplacement MainMenuDisplacement { get; set; }
        public string Displacement {
            get {
                return MainMenuDisplacement.ToString().ToLower();
            }
        }
        public IEnumerable<MenuBaseDefinition> Leafs { get; set; }

        public List<ModuleDefinition> Modules { get; set; }  

        public override string ToString() {
            return string.Format("Leafs: {0}, MainMenuDisplacement: {1}", Leafs, MainMenuDisplacement);
        }
    }
}
