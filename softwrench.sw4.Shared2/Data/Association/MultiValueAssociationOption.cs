using System.Collections.Generic;
using softwrench.sW4.Shared2.Data;

namespace softwrench.sw4.Shared2.Data.Association {
    public class MultiValueAssociationOption : GenericAssociationOption {

        private readonly IDictionary<string, object> _extrafields;
        private readonly bool _forceDistinctOptions;

        public MultiValueAssociationOption(string value, string label, DataMapDefinition extrafields, bool forceDistinctOptions = true)
            : base(value, label) {
                _extrafields = extrafields.Fields;
                _forceDistinctOptions = forceDistinctOptions;
            }

        public MultiValueAssociationOption(string value, string label, IDictionary<string, object> fields, bool forceDistinctOptions = true) 
            : base(value, label) {
            _extrafields = fields;
            _forceDistinctOptions = forceDistinctOptions;
            }

        public IDictionary<string, object> Extrafields {
            get { return _extrafields; }
        }

       public bool ForceDistinctOptions {
            get { return _forceDistinctOptions; }
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
