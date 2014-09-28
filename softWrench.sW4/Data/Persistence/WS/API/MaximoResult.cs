namespace softWrench.sW4.Data.Persistence.WS.API {
    public class MaximoResult {
        public string Id { get; set; }
        public object ResultObject { get; set; }

        public MaximoResult(string id, object resultObject) {
            Id = id;
            ResultObject = resultObject;
        }
    }
}
