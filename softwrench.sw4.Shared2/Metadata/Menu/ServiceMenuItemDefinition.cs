using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;

namespace softwrench.sw4.Shared2.Metadata.Menu {
    public class ServiceMenuItemDefinition : MenuBaseDefinition, IMenuLeaf {
        public string Service { get; set; }
        public string Method { get; set; }

        public ServiceMenuItemDefinition(string id, string title, string role, string tooltip, string icon, string customizationPosition, 
            string service, string method) : base(id, title, role, tooltip, icon, customizationPosition) {
            Service = service;
            Method = method;
        }
    }
}
