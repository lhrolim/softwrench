using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using System;

namespace softWrench.sW4.Data.API {
    public class ActionRedirectResponse : IApplicationResponse {

        public string Action { get; set; }
        public string Controller { get; set; }

        public string Parameters { get; set; }



        public string RedirectURL { get; set; }
        public string Title { get; set; }
        public string CrudSubTemplate { get; set; }
        public string SuccessMessage { get; set; }
        public long RequestTimeStamp { get; set; }
        public DateTime TimeStamp { get; set; }

        public string Type {
            get { return GetType().Name; }
        }

        #region uselessmethodstokeephierarchy...
        
        public ApplicationSchemaDefinition Schema { get; set; }
        public string CachedSchemaId {
            get; set;
        }

        public string Mode { get; set; }
        public string ApplicationName { get; private set; }

        #endregion
    }
}
