using System.Collections.Generic;
using softwrench.sw4.Shared2.Data.Association;


namespace softWrench.sW4.Data.API.Association {
    public class BaseAssociationUpdateResult {

        private readonly IEnumerable<IAssociationOption> _associationData;


        public BaseAssociationUpdateResult(IEnumerable<IAssociationOption> associationData) {
            _associationData = associationData;
        }

        public IEnumerable<IAssociationOption> AssociationData { get { return _associationData; } }
    }
}
