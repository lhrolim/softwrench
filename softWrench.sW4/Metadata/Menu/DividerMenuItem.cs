using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softWrench.sW4.Metadata.Menu {
    public class DividerMenuItem : MenuBaseDefinition, IMenuLeaf {
        public DividerMenuItem(string id, string title, string role, string tooltip, string icon,string customizationPosition)
            : base(id, title, role, tooltip, icon, customizationPosition) {
        }

        public DividerMenuItem()
            : base(null, null, null, null, null,null) {
        }
    }
}
