using System.Collections.Generic;
using System.Text;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.util {
    class BatchRedirectionHelper {

        public static PaginatedSearchRequestDto BuildDTO(Batch resultBatch) {
            var dto = new PaginatedSearchRequestDto();

            var resultsBySiteId = new Dictionary<string, IList<string>>();
            foreach (var result in resultBatch.TargetResults) {
                if (!resultsBySiteId.ContainsKey(result.SiteId)) {
                    resultsBySiteId[result.SiteId] = new List<string>();
                }
                resultsBySiteId[result.SiteId].Add(result.UserId);
            }
            var sb = new StringBuilder();
            foreach (var siteIdWoNums in resultsBySiteId) {
                sb.AppendFormat("(workorder.siteid = '{0}' and wonum in ({1}))", siteIdWoNums.Key,
                    BaseQueryUtil.GenerateInString(siteIdWoNums.Value));
                sb.Append(" or ");
            }
            dto.WhereClause = sb.ToString(0, sb.Length - 4);
            return dto;
        }

    }
}
