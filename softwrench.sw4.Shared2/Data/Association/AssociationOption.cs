using System;

namespace softwrench.sw4.Shared2.Data.Association {

    /// <summary>
    /// Represents a association Option, which should be displayed in a combobox, which contains the label to display on the screen and its underlying value
    /// </summary>
    public class AssociationOption : GenericAssociationOption {

        public AssociationOption(String value, string label, string help = null, string enableExpression= null)
            : base(value, label, help, enableExpression) {

        }

        protected bool Equals(GenericAssociationOption other) {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GenericAssociationOption)obj);
        }

        public override int GetHashCode() {
            return (Value != null ? Value.GetHashCode() : 0);
        }

    }
}
