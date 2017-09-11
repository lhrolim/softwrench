using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API.Composition {

    public class CompositionDetailFetchRequestDTO {

        /// <summary>
        /// Used whenever we are on a composition context, for instance, trying to expand or see the details of a composition of a given entry
        /// </summary>
        public ApplicationMetadataSchemaKey RootApplicationKey { get; set; }

        public string CompositionKey { get; set; }

    }
}
