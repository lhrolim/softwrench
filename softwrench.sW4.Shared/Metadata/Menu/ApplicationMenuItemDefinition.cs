using softwrench.sW4.Shared.Metadata.Applications.Schema;
using softwrench.sW4.Shared.Metadata.Menu.Interfaces;

namespace softwrench.sW4.Shared.Metadata.Menu {
    public class ApplicationMenuItemDefinition : MenuBaseDefinition, IMenuLeaf {

        public string Application { get; set; }

        public string Schema { get; set; }
        //        [JsonConverter(typeof(StringEnumConverter))]
        public SchemaMode Mode { get; set; }

        public ApplicationMenuItemDefinition() { }

        public ApplicationMenuItemDefinition(string id, string title, string role, string tooltip, string icon, string application, string schema, SchemaMode mode)
            : base(id, title, role, tooltip, icon) {
            Application = application;
            Schema = schema;
            Mode = mode;
        }
    }
}
