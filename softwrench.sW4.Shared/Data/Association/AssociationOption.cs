using System;

namespace softwrench.sW4.Shared.Metadata.Applications.Association {

    /// <summary>
    /// Represents a association Option, which should be displayed in a combobox, which contains the label to display on the screen and its underlying value
    /// </summary>
    public class AssociationOption : GenericAssociationOption<String> {

        public AssociationOption(String value, string label)
            : base(value, label) {
        }

    }
}
