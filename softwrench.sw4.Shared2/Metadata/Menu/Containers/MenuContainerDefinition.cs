﻿using System.Collections.Generic;
using System.Linq;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softwrench.sW4.Shared2.Metadata.Menu.Containers {
    public class MenuContainerDefinition : MenuBaseDefinition {
        public IEnumerable<MenuBaseDefinition> Leafs { get; set; }

        public string Action { get; set; }
        public string Controller { get; set; }

        public MenuContainerDefinition() { }

        public MenuContainerDefinition(string id, string title, string role, string tooltip, string icon, string module, string controller, string action, IEnumerable<MenuBaseDefinition> leafs)
            : base(id, title, role, tooltip, icon) {
            Module = module;
            var menuBaseDefinitions = leafs as MenuBaseDefinition[] ?? leafs.ToArray();
            Leafs = menuBaseDefinitions;
            foreach (var leaf in menuBaseDefinitions) {
                if (leaf.Module == null) {
                    leaf.Module = module;
                }
            }
            Action = action;
            Controller = controller;
        }
    }
}
