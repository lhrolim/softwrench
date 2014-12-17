using System;
using System.Linq;
using System.Collections.Generic;

namespace softwrench.sw4.Shared2.Data.Association {
    public class GenericAssociationOption : IComparable, IAssociationOption {

        public string Value { get; set; }

        public string Label { get; set; }

        public GenericAssociationOption() {
        }

        public GenericAssociationOption(string value, string label) {
            Value = value;
            Label = label;
        }

        public int CompareTo(GenericAssociationOption other) {
            return System.String.Compare(Label, other.Label, System.StringComparison.Ordinal);
        }

        public String Type {
            get { return this.GetType().Name; }
        }

        public override string ToString() {
            return string.Format("Value: {0}, Label: {1}", Value, Label);
        }

        public virtual int CompareTo(object obj) {
            var other = (GenericAssociationOption)obj;
            var labelComparison = System.String.Compare(Label, other.Label, System.StringComparison.Ordinal);
            if (labelComparison == 0) {
                return System.String.Compare(Value, other.Value, System.StringComparison.Ordinal);
            }
            return labelComparison;
        }
    }

    public class OptionComparer : IEqualityComparer<GenericAssociationOption> {

        public bool Equals(GenericAssociationOption x, GenericAssociationOption y) {
            return x.CompareTo(y) == 0;
        }

        public int GetHashCode(GenericAssociationOption obj) {
            return obj.Value.GetHashCode() & obj.Label.GetHashCode();
        }
    }
}
