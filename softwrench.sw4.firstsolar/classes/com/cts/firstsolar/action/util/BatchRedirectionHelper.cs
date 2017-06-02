using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.simpleinjector;
using softwrench.sw4.batch.api.entities;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Security.Context;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.util {
    public class BatchRedirectionHelper : ISingletonComponent {

        private readonly IContextLookuper _contextLookuper;
        private readonly MaximoHibernateDAO _dao;

        private const string MockQuery = "select top 10 workorderid from workorder";

        public BatchRedirectionHelper(IContextLookuper contextLookuper, MaximoHibernateDAO dao) {
            _contextLookuper = contextLookuper;
            _dao = dao;
        }

        public PaginatedSearchRequestDto BuildDTO(Batch resultBatch) {
            if (_contextLookuper.LookupContext().MockMaximo) {
                return MockDTO();
            }

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

        private PaginatedSearchRequestDto MockDTO() {
            var queryResult = _dao.FindByNativeQuery(MockQuery);

            var whereClause = new StringBuilder();
            whereClause.Append("workorderid in (");
            var idList = queryResult.Select(row => row["workorderid"]).ToList();
            whereClause.Append(string.Join(", ", idList));
            whereClause.Append(")");

            return new PaginatedSearchRequestDto { PageSize = 10, FilterFixedWhereClause = whereClause.ToString() }; ;
        }

    }
}
