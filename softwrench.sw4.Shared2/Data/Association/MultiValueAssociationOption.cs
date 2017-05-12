using System.Collections.Generic;
using JetBrains.Annotations;
using softwrench.sW4.Shared2.Data;

namespace softwrench.sw4.Shared2.Data.Association {
    public class MultiValueAssociationOption : GenericAssociationOption {
        public MultiValueAssociationOption() {
        }

        public MultiValueAssociationOption(string value, string label, DataMapDefinition extrafields, bool forceDistinctOptions = true, string help = null, string enableExpression = null)
            : base(value, label, help, enableExpression) {
            Extrafields = extrafields.Fields;
            ForceDistinctOptions = forceDistinctOptions;
        }

        public MultiValueAssociationOption(string value, string label, [CanBeNull]IDictionary<string, object> fields, bool forceDistinctOptions = true, string help = null, string enableExpression = null)
            : base(value, label, help, enableExpression) {
            Extrafields = fields;
            ForceDistinctOptions = forceDistinctOptions;
        }

        [CanBeNull]
        public IDictionary<string, object> Extrafields {
            get; set;
        }

        public bool ForceDistinctOptions {
            get; set;
        }

        public override int CompareTo(object obj) {
            var baseResult = base.CompareTo(obj);
            if (baseResult == 0 && ForceDistinctOptions == false) {
                return 1;
            }
            return baseResult;
        }
    }
}
