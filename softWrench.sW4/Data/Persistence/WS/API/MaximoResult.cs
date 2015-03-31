namespace softWrench.sW4.Data.Persistence.WS.API {
 
    public class TargetResult {
        public string UserId { get; set; }
        public object ResultObject { get; set; }

        public TargetResult(string userId, object resultObject) {
            UserId = userId;
            ResultObject = resultObject;
        }
    }
}
