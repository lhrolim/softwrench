using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using softwrench.sw4.Shared2.Data.Association;

namespace softWrench.sW4.Data.API.Association {
    public class DependentAssociationLoadingResult {

        private IDictionary<string, IEnumerable<IAssociationOption>> _eagerOptions = new Dictionary<string, IEnumerable<IAssociationOption>>();

        /// <summary>
        /// Lists of eager collections, containing n options here.
        /// 
        /// The type of the TEager can vary across the different implementations, since theoritically,
        /// compositions can have different option lists for each of their rows
        /// 
        /// </summary>
        [NotNull]
        public IDictionary<string, IEnumerable<IAssociationOption>> EagerOptions {
            get { return _eagerOptions; }
            set { _eagerOptions = value; }
        }


    }
}
