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


    public class ValueComparer : IEqualityComparer<IAssociationOption> {

        public bool Equals(IAssociationOption x, IAssociationOption y) {

            //Check whether the compared objects reference the same data. 
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null. 
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Value == y.Value;
        }

        public int GetHashCode(IAssociationOption item) {
            //Check whether the object is null 
            if (Object.ReferenceEquals(item, null)) return 0;

            //Get hash code for the Name field if it is not null. 
            int hashProductName = item.Value == null ? 0 : item.Value.GetHashCode();

            //Get hash code for the Code field. 
            int hashProductCode = item.Value.GetHashCode();

            //Calculate the hash code for the product. 
            return hashProductName ^ hashProductCode;
        }
    }
}
