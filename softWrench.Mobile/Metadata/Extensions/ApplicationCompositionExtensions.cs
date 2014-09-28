using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Metadata.Extensions {
    internal static class ApplicationCompositionExtensions {

        public static ApplicationSchemaDefinition To(this ApplicationCompositionDefinition definition) {
            return (ApplicationSchemaDefinition)definition.ExtensionParameter("To");
        }

        public static void To(this ApplicationCompositionDefinition definition, ApplicationSchemaDefinition schema) {
            definition.ExtensionParameter("To", schema);
        }

        public static string Relationship(this ApplicationCompositionDefinition definition) {
            //currently the server is returning a _ at the end of the string, due to a (probably)bad design decision.
            //TODO: fix that on the server later
            return definition.Relationship.Replace("_", "");
        }


    }
}
