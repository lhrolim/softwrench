using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.API.Association {

    /// <summary>
    /// Used to retrieve a single association
    /// </summary>
    public class SingleAssociationPrefetcherRequest :IAssociationPrefetcherRequest{
        public string AssociationsToFetch { get; set; }
        public bool IsShowMoreMode { get; set; }
    }
}
