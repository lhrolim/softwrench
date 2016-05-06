using System.ComponentModel;

namespace softwrench.sw4.Shared2.Metadata.Applications.Filter {

    public class MetadataBooleanFilter : BaseMetadataFilter {
        public string PreSelected {
            get; set;
        }

        public string TrueLabel {
            get; set;
        }

        public string TrueValue {
            get; set;
        }

        public string FalseLabel {
            get; set;
        }

        public string FalseValue {
            get; set;
        }

        public MetadataBooleanFilter(string attribute, string label, string icon, string position, string tooltip, string whereClause, string preSelected, string trueLabel, string trueValue, string falseLabel, string falseValue)
            : base(attribute, label, icon, position, tooltip, whereClause) {
            PreSelected = preSelected;
            TrueLabel = trueLabel;
            TrueValue = trueValue;
            FalseLabel = falseLabel;
            FalseValue = falseValue;
        }
    }
}
