using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence {
    class NHibernatePaginationUtil {

        const string ManualPaginationUnionTemplate = @"select * from(
            select tempresult.*, rownumber() over({0}) as rownum from(
            {1}
            ) as tempresult) paging 
            where {2}";

        private const string ManualPaginationTemplate =
            "select * from (select rownumber() over({0}) as rownum, {1}) as tempresult where {2}";

        //needed because nhibernate paging is buggy
        public static string ApplyManualPaging(string queryst, PaginationData paginationData) {
            var pageSize = paginationData.PageSize;
            var firstValue = (paginationData.PageNumber - 1) * pageSize;
            var hasUnion = queryst.Contains("union") || queryst.Contains("UNION");
            var templateToUse = hasUnion ? ManualPaginationUnionTemplate : ManualPaginationTemplate;
            var pagingCondition = hasUnion ? "paging.rownum" : "rownum";
            var orderBy1 = paginationData.SortString;
            var orderBy = orderBy1;
            if (!hasUnion) {
                //need to remove the select keyword here
                queryst = queryst.Substring("select".Length);
            }
            queryst = queryst.Replace(orderBy, "");
            var paginationCondition = pagingCondition + " <= " + pageSize;
            if (firstValue != 0) {
                paginationCondition = pagingCondition + " between " + (firstValue + 1) + " and " + (firstValue + pageSize);
            }
            return templateToUse.Fmt(orderBy, queryst, paginationCondition);
        }
    }
}
