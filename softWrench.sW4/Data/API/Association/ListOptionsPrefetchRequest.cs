using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.API.Association {
    /// <summary>
    /// This class should be used for optionfields inside of a list schema
    /// </summary>
    internal class ListOptionsPrefetchRequest : IAssociationPrefetcherRequest {
        public string AssociationsToFetch { get; set; }
    }
}
