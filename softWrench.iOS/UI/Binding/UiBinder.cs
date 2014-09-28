using System.Collections.Generic;
using System.Linq;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Applications;
using softWrench.Mobile.Metadata.Applications.UI;
using softWrench.Mobile.Metadata.Extensions;
using softWrench.Mobile.UI.Binding;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.iOS.UI.Binding {
    internal static class UiBinder {
        private static IValueProvider CreateValueProvider(ApplicationFieldDefinition fieldMetadata) {
            var isHidden = fieldMetadata.Widget() is HiddenWidget;

            return new ValueProvider(isHidden);
        }

        public static FormBinding Bind(DataMap dataMap, ApplicationSchemaDefinition application, bool isNew) {
            return softWrench.Mobile.UI.Binding.UiBinder.Bind(dataMap, application, isNew, CreateValueProvider);
        }

        public static IEnumerable<FieldBinding> VisibleFields(this FormBinding form) {
            return form
                .Fields
                .Where(f => false == ((ValueProvider)f.ValueProvider).IsHidden);
        }
    }
}
