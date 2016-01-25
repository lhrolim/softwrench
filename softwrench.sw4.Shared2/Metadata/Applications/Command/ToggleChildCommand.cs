using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared2.Metadata.Applications.Command;

namespace softwrench.sw4.Shared2.Metadata.Applications.Command {
    public class ToggleChildCommand : ApplicationCommand {
        public ToggleChildCommand(string id, string label, string service, string method, string role, string stereotype,
            string showExpression, string enableExpression, string successMessage,
            string nextSchemaId, string scopeParameters, string properties, string defaultPosition, string icon,
            string tooltip, string cssClasses, bool primary, bool pressed) : base(id, label, service, method, role, stereotype,
            showExpression, enableExpression, successMessage,
            nextSchemaId, scopeParameters, properties, defaultPosition, icon,
            tooltip, cssClasses, primary)
        {
            Pressed = pressed;
        }

        public bool Pressed {
            get; set;
        }
    }
}
