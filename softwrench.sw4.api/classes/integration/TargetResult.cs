using softWrench.sW4.Metadata.Applications;

namespace softWrench.sW4.Data.Persistence.WS.API {
 
    public class TargetResult {
        // Unique Id use to identify the record; it might be different than the userid
        public string Id { get; set; }
        public string UserId { get; set; }
        public object ResultObject { get; set; }
        public string SuccessMessage { get; set; }

        public ApplicationMetadata NextApplication { get; set; }

        public string NextController { get; private set; }

        public string NextAction { get; private set; }
        public string SiteId { get; set; }

        public TargetResult(string id, string userId, object resultObject, string successMessage = null,string siteId=null) {
            Id = id;
            UserId = userId;
            ResultObject = resultObject;
            SuccessMessage = successMessage;
            SiteId = siteId;
        }
    }
}
