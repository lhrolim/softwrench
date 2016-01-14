﻿using cts.commons.portable.Util;
using cts.commons.Util;

namespace cts.commons.persistence {
    class NHibernatePaginationUtil {

        const string ManualPaginationUnionTemplate = @"select * from(
            select tempresult.*, rownumber() over(order by {0} ) as rownum from(
            {1}
            ) as tempresult) paging 
            where {2}";

        private const string ManualPaginationTemplate =
            "select * from (select rownumber() over(order by {0}) as rownum, {1}) as tempresult where {2}";

        //needed because nhibernate paging is buggy
        public static string ApplyManualPaging(string queryst, IPaginationData paginationData) {
            var pageSize = paginationData.PageSize;
            var firstValue = (paginationData.PageNumber - 1) * pageSize;
            var hasUnion = queryst.Contains("union");
            var templateToUse = hasUnion ? ManualPaginationUnionTemplate : ManualPaginationTemplate;
            var pagingCondition = hasUnion ? "paging.rownum" : "rownum";
            var orderBy1 = hasUnion ? paginationData.OrderByColumn : paginationData.QualifiedOrderByColumn;
            var orderBy = orderBy1;
            if (!hasUnion) {
                //need to remove the select keyword here
                queryst = queryst.Substring("select".Length);
            }
            var paginationCondition = pagingCondition + " <= " + pageSize;
            if (firstValue != 0) {
                paginationCondition = pagingCondition + " between " + (firstValue + 1) + " and " + (firstValue + pageSize);
            }
            return templateToUse.Fmt(orderBy, queryst, paginationCondition);
        }
    }
}