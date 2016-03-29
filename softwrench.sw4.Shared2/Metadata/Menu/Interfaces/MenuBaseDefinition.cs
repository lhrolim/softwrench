using System;

namespace softwrench.sW4.Shared2.Metadata.Menu.Interfaces {
    public abstract class MenuBaseDefinition {

        public string Id { get; set; }
        public string Title { get; set; }
        public string Role { get; set; }
        public string Tooltip { get; set; }
        public string Icon { get; set; }

        public Boolean RoleDefinedByParent { get; set; }

        public string Module { get; set; }

        public string CustomizationPosition { get; set; }

        public string PermissionExpresion {
            get; set;
        }

        public MenuBaseDefinition() {

        }

        protected MenuBaseDefinition(string id, string title, string role, string tooltip, string icon,string customizationPosition) {
            Id = id;
            Title = title;
            Role = role;
            Tooltip = tooltip;
            Icon = icon;
            CustomizationPosition = customizationPosition;
        }


        public string Type { get { return GetType().Name; } }
        public Boolean Leaf { get { return this is IMenuLeaf; } }

        public override string ToString() {
            return string.Format("Title: {0}, Id: {1}", Title, Id);
        }

        public MenuBaseDefinition ShallowCopy() {
            return (MenuBaseDefinition)MemberwiseClone();
        }
    }
}
