using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Menu.Containers {
    public interface IMenuRootDefinition {
        IEnumerable<MenuBaseDefinition> Leafs { get; set; }
    }
}