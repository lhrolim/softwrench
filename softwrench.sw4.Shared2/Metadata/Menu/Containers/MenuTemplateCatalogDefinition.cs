using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Menu.Containers {

    public class MenuTemplateCatalog :IMenuRootDefinition{

        public IEnumerable<MenuBaseDefinition> Leafs { get; set; }

        public MenuTemplateCatalog(IEnumerable<MenuBaseDefinition> items) {
            Leafs = items;
        }



    }
}
