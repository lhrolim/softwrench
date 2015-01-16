namespace softWrench.sW4.Data.Persistence.WS.API {
 
    public class TargetResult {
        public string Id { get; set; }
        public object ResultObject { get; set; }

        public TargetResult(string id, object resultObject) {
            Id = id;
            ResultObject = resultObject;
        }
    }
}
