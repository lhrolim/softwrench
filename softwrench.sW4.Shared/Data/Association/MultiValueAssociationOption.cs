using System.Collections.Generic;
using softwrench.sW4.Shared.Metadata.Applications.Association;

namespace softwrench.sW4.Shared.Data.Association {
    public class MultiValueAssociationOption : GenericAssociationOption<string> {

        private readonly IDictionary<string, object> _extrafields;

        public MultiValueAssociationOption(DataMapDefinition value, string label)
            : base(value.GetAttribute("value",true).ToString(), label) {
            _extrafields = value.Fields;
        }

        public IDictionary<string, object> Extrafields {
            get { return _extrafields; }
        }

      
    }
}
