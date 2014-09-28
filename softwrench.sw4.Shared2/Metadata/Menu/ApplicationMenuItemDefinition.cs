using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Menu.Interfaces;
using System.Collections.Generic;

namespace softwrench.sW4.Shared2.Metadata.Menu {
    public class ApplicationMenuItemDefinition : MenuBaseDefinition, IMenuLeaf {

        public string Application { get; set; }

        public string Schema { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public SchemaMode Mode { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
        
        public ApplicationMenuItemDefinition() { }

        public ApplicationMenuItemDefinition(string id, string title, string role, string tooltip, string icon, string application,
            string schema, SchemaMode mode, IDictionary<string, string> parameters)
            : base(id, title, role, tooltip, icon) {
            Application = application;
            Schema = schema;
            Mode = mode;
            Parameters = parameters;
        }
    }
}
