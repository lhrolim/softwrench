namespace cts.commons.persistence
{
    public interface IPaginationData
    {
        int PageSize { get; set; }
        int PageNumber { get; set; }
        int NumberOfPages { get; set; }
        //this is needed because nhibernate builds buggy pagination queries, not appending the the alias on the over query. 
        //in some scenarios this may lead to ambigous column definition
        string QualifiedOrderByColumn { get; set; }
        string OrderByColumn { get; set; }
    }
}