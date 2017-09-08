using System;
using softwrench.sw4.api.classes.integration;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.sW4.Data.API.Response {
    public class NotFoundResponse : IApplicationResponse {

        public string AliasURL { get; set; }

        public string Type {
            get {
                return typeof(NotFoundResponse).Name;
            }
        }

        #region uselessmethodstokeephierarchy...
        public ApplicationSchemaDefinition Schema { get; set; }
        public string CachedSchemaId { get; set; }
        public string Mode { get; set; }
        public string ApplicationName { get; private set; }
        public string Id { get; private set; }
        public IErrorDto WarningDto { get; set; }

        public string RedirectURL { get; set; }
        public string Title { get; set; }
        public string CrudSubTemplate { get; set; }
        public string SuccessMessage { get; set; }
        public DateTime TimeStamp { get; set; }
        #endregion
    }
}
