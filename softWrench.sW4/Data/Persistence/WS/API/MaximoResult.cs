namespace softWrench.sW4.Data.Persistence.WS.API {
 
    public class TargetResult {
        // Unique Id use to identify the record; it might be different than the userid
        public string Id { get; set; }
        public string UserId { get; set; }
        public object ResultObject { get; set; }

        public TargetResult(string id, string userId, object resultObject) {
            Id = id;
            UserId = userId;
            ResultObject = resultObject;
        }
    }
}
