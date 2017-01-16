using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using softwrench.sw4.Shared2.Metadata.Menu.Containers;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using softwrench.sw4.Shared2.Metadata.Modules;

namespace softwrench.sW4.Shared2.Metadata.Menu.Containers {

    public class MenuDefinition : IMenuRootDefinition {
        public enum MenuDisplacement {
            Vertical, Horizontal, Breadcrumb,
        }

        private List<MenuBaseDefinition> _cachedExplodedLeafs;

        private object lockObj = new object();

        public MenuDefinition() {
        }

        public MenuDefinition(IEnumerable<MenuBaseDefinition> leafs, MenuDisplacement mainMenuDisplacement, string indexItemId) {
            MainMenuDisplacement = mainMenuDisplacement;
            Leafs = leafs;
            ItemindexId = indexItemId;
        }

        public MenuDefinition(IEnumerable<MenuBaseDefinition> leafs, String mainMenuDisplacement, string indexItemId) {
            MainMenuDisplacement = (MenuDisplacement)Enum.Parse(typeof(MenuDisplacement), mainMenuDisplacement, true);
            Leafs = leafs;
            ItemindexId = indexItemId;
        }

        public MenuDisplacement MainMenuDisplacement {
            get; set;
        }
        public string Displacement {
            get {
                return MainMenuDisplacement.ToString().ToLower();
            }
        }
        public IEnumerable<MenuBaseDefinition> Leafs {
            get; set;
        }

        public IEnumerable<MenuBaseDefinition> ExplodedLeafs {
            get {
                lock (lockObj) {
                    if (_cachedExplodedLeafs != null) {
                        return _cachedExplodedLeafs;
                    }
                    _cachedExplodedLeafs = new List<MenuBaseDefinition>();

                    foreach (var menuBaseDefinition in Leafs) {
                        if (menuBaseDefinition.Leaf) {
                            _cachedExplodedLeafs.Add(menuBaseDefinition);
                        } else {
                            _cachedExplodedLeafs.AddRange(((MenuContainerDefinition)menuBaseDefinition).ExplodedLeafs);
                        }
                    }

                    return _cachedExplodedLeafs;
                }
            }
        }

        public List<ModuleDefinition> Modules {
            get; set;
        }
        public string ItemindexId {
            get; set;
        }

        public override string ToString() {
            return string.Format("Leafs: {0}, MainMenuDisplacement: {1}", Leafs, MainMenuDisplacement);
        }
    }
}

