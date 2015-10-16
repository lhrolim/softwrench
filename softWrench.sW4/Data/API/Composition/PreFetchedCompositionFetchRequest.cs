using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.API.Composition {
    public class PreFetchedCompositionFetchRequest : CompositionFetchRequest {

        public IReadOnlyList<AttributeHolder> PrefetchEntities {
            get; set;
        }

        public PreFetchedCompositionFetchRequest(IReadOnlyList<AttributeHolder> entities) {
            PrefetchEntities = entities; PrefetchEntities = entities;
        }
    }


}
