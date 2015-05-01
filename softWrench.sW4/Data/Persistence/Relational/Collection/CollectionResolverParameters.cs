using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Metadata.Applications;
using softwrench.sW4.Shared2.Data;

namespace softWrench.sW4.Data.Persistence.Relational.Collection {

    public class CollectionResolverParameters {

        public ApplicationMetadata ApplicationMetadata { get; set; }

        public IDictionary<string, long?> RowstampMap { get; set; }
        public IEnumerable<AttributeHolder> ParentEntities { get; set; }


    }
}
