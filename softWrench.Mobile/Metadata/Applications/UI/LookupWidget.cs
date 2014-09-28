using System;
using System.Collections.Generic;
using softwrench.sW4.Shared2.Metadata.Applications.UI;

namespace softWrench.Mobile.Metadata.Applications.UI {
    public sealed class LookupWidget : LookupWidgetDefinition, IWidget {

        private readonly LookupWidgetDefinition _definition;

        public LookupWidget(LookupWidgetDefinition definition)
            : base(definition.SourceApplication, definition.SourceField, definition.SourceDisplay, definition.TargetField, definition.TargetQualifier, definition.Filters) {
            _definition = definition;
        }

        public string Type { get { return GetType().Name; } }

        public string Format(string value) {
            return value;
        }

        public bool Validate(string value, out string error) {
            error = null;
            return true;
        }




    }
}
