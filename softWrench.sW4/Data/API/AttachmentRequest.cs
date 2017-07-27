using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.API {
    public class AttachmentRequest : IDataRequest {

        public ApplicationMetadataSchemaKey Key { get; set; }

        public string Title { get; set; }

        public IDictionary<string, object> CustomParameters { get; set; }
        
        public string CommandId { get; set; }

        public string ParentId { get; set; }

        public string ParentApplication { get; set; }

        public string ParentSchemaId { get; set; }
    }
}
