using System.Collections.Generic;
using softwrench.sW4.Shared2.Data;

namespace softwrench.sw4.Shared2.Data.Association {
    public class MultiValueAssociationOption : GenericAssociationOption {

        private readonly IDictionary<string, object> _extrafields;

        public MultiValueAssociationOption(string value, string label, DataMapDefinition extrafields)
            : base(value, label) {
                _extrafields = extrafields.Fields;
        }

        public MultiValueAssociationOption(string value, string label, IDictionary<string, object> fields)
            : base(value, label) {
            _extrafields = fields;
        }

        public IDictionary<string, object> Extrafields {
            get { return _extrafields; }
        }

      
    }
}
