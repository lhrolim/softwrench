namespace softwrench.sw4.Shared2.Metadata.Entity {
    public interface IQueryHolder {

        string Query { get; }

        string GetQueryReplacingMarkers(string entityName, string fromValue=null);
    }
}
