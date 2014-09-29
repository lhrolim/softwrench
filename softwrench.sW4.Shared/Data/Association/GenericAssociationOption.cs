using System;

namespace softwrench.sW4.Shared.Metadata.Applications.Association {
    public class GenericAssociationOption<T> : IComparable,IAssociationOption {

        public T Value { get; set; }

        public string Label { get; set; }

        public GenericAssociationOption() {

        }

        public GenericAssociationOption(T value, string label) {
            Value = value;
            Label = label;
        }

        public int CompareTo(GenericAssociationOption<T> other) {
            return System.String.Compare(Label, other.Label, System.StringComparison.Ordinal);
        }

        public String Type {
            get { return this.GetType().Name; }
        }

        public override string ToString() {
            return string.Format("Value: {0}, Label: {1}", Value, Label);
        }

        public int CompareTo(object obj)
        {
            var other = (IAssociationOption)obj;
            return System.String.Compare(Label, other.Label, System.StringComparison.Ordinal);
        }
    }
}
