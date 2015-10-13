using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using System;

namespace softWrench.sW4.Data.API {
    public class BlankApplicationResponse : IApplicationResponse {
        public string RedirectURL { get; set; }
        public string Title { get; set; }
        public string CrudSubTemplate { get; set; }
        public string SuccessMessage { get; set; }
        public long RequestTimeStamp { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Type { get; private set; }
        public ApplicationSchemaDefinition Schema { get; set; }
        public string CachedSchemaId { get; set; }
        public string Mode { get; set; }
        public string ApplicationName { get; private set; }
    }
}
