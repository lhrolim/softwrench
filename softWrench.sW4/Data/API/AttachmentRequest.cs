using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using System;
using System.Collections.Generic;

namespace softWrench.sW4.Data.API {
    public class AttachmentRequest : IDataRequest {

        public ApplicationMetadataSchemaKey Key { get; set; }

        public String Title { get; set; }

        public IDictionary<string, string> CustomParameters { get; set; }
        
        public string CommandId { get; set; }

        public String ParentId { get; set; }

        public String ParentApplication { get; set; }

        public String ParentSchemaId { get; set; }
    }
}
