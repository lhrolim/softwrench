using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Compositions;
using softWrench.sW4.Metadata.Stereotypes.Schema;

namespace softWrench.sW4.Metadata.Applications.Schema {
    static class ApplicationCompositionSchemaExtension {

        public static bool HasAtLeastOneVisibleFieldForSearch(this ApplicationCompositionDefinition composition) {
            var schema = composition.Schema.Schemas.List;
            var quickSearchFields = schema.GetProperty(ApplicationSchemaPropertiesCatalog.ListQuickSearchFields);
            if (quickSearchFields != null) {
                return !string.IsNullOrEmpty(quickSearchFields);
            }
            return schema.NonHiddenFields.Any(f => f.IsTextField);
        }
    }
}
