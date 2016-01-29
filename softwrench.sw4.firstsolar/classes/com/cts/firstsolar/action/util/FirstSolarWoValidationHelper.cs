using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.simpleinjector;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.dto;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.util {
    public class FirstSolarWoValidationHelper : ISingletonComponent {

        private readonly MaximoHibernateDAO _dao;

        private const string BaseBatchLocationQuery = "select location,wonum from workorder where status not in('CLOSED','COMPLETED') and ({0})";

        private const string BaseSingleLocationProjectionQuery = "select wonum,description,status from workorder where ";

        private const string BaseAssetQuery =
            "select assetnum,wonum,description,location from workorder where status not in('CLOSED','COMPLETED') and assetnum in ({0})";
        //this is needed to return to the client side the fixedwhereclause for the modal
        private const string BaseSingleLocationWhereQuery = "status not in('CLOSED','COMPLETED') and location = '{0}'";

        public FirstSolarWoValidationHelper(MaximoHibernateDAO dao) {
            _dao = dao;
        }


        public IDictionary<string, List<string>> ValidateIdsThatHaveWorkorders(FirstSolarBatchType batchType, ICollection<MultiValueAssociationOption> items, string classification) {
            var userIdFieldName = batchType.GetUserIdName();

            var sb = new StringBuilder();
            if (batchType.Equals(FirstSolarBatchType.Location)) {
                var orQueryString = BaseQueryUtil.GenerateOrLikeString("location", items.Select(i => i.Value + "%"));
                sb.AppendFormat(BaseBatchLocationQuery, orQueryString);
            } else {
                var inQueryString = BaseQueryUtil.GenerateInString(items.Select(i => i.Value));
                sb.AppendFormat(BaseAssetQuery, inQueryString);
            }

            if (classification != null) {
                sb.AppendFormat(" and classstructureid = '{0}'", classification);
            }
            sb.AppendFormat("order by {0} asc", userIdFieldName);
            var queryResult = _dao.FindByNativeQuery(sb.ToString());

            var result = new Dictionary<string, List<string>>();

            foreach (var row in queryResult) {
                var itemId = row[userIdFieldName];
                var wonum = row["wonum"];
                var matchingResult = result.Keys.FirstOrDefault(f => itemId.StartsWith(f));
                if (matchingResult == null) {
                    var list = new List<string>() { wonum };
                    result.Add(itemId, list);
                } else {
                    result[matchingResult].Add(wonum);
                }
            }
            return result;
        }


        public ApplicationListResult GetRelatedLocationWorkorders(string location, string classification) {
            var sb = new StringBuilder();
            sb.AppendFormat(BaseSingleLocationWhereQuery, location);
            if (classification != null) {
                sb.AppendFormat("and classstructureid = {0}", classification);
            }
            var queryResult = _dao.FindByNativeQuery(BaseSingleLocationProjectionQuery + sb);

            var dataSet = DataSetProvider.GetInstance().LookupDataSet("workorder", "relatedwoconfirmationlist");
            var app = MetadataProvider.Application("workorder").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("relatedwoconfirmationlist"));
            PaginatedSearchRequestDto dto = PaginatedSearchRequestDto.DefaultInstance(app.Schema);
            dto.PageSize = 10;
            dto.FilterFixedWhereClause = sb.ToString();
            return dataSet.GetList(app, dto);
        }



    }
}
