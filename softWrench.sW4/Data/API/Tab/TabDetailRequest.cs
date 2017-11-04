using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Operation;

namespace softWrench.sW4.Data.API {
    public class TabDetailRequest : IDataRequest{


        public ApplicationMetadataSchemaKey Key { get; set; }
        public string Title { get; set; }
        public IDictionary<string, object> CustomParameters { get; set; }
        public string CommandId { get; set; }

        public string Id { get; set; }

        public string TabId { get; set; }

        public CrudOperationData CrudData { get; set; }

    }
}
