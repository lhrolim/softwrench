using System;

namespace softwrench.sw4.Shared2.Data.Association {

    /// <summary>
    /// Represents a association Option, which should be displayed in a combobox, which contains the label to display on the screen and its underlying value
    /// </summary>
    public class AssociationOption : GenericAssociationOption {

        public AssociationOption(String value, string label)
            : base(value, label) {
        }

    }
}
