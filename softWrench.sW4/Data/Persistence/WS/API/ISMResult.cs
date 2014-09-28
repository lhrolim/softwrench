namespace softWrench.sW4.Data.Persistence.WS.API
{
    public class ISMResult
    {
        public string Id { get; set; }
        public object ResultObject { get; set; }

        public ISMResult(string id, object resultObject)
        {
            Id = id;
            ResultObject = resultObject;
        }
    }
}
