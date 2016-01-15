using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Sliced;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.util {
    public class FirstSolarWoValidationHelper : ISingletonComponent {

        private readonly MaximoHibernateDAO _dao;

        private const string BaseBatchLocationQuery = "select location,wonum from workorder where status not in('CLOSED','COMPLETED') and location in ({0})";

        private const string BaseSingleLocationProjectionQuery = "select wonum,description,status from workorder where ";

        private const string BaseAssetQuery =
            "select assetnum,wonum,description from workorder where status not in('CLOSED','COMPLETED') and assetnum in ({0})";
        //this is needed to return to the client side the fixedwhereclause for the modal
        private const string BaseSingleLocationWhereQuery = "status not in('CLOSED','COMPLETED') and location = '{0}'";

        public FirstSolarWoValidationHelper(MaximoHibernateDAO dao) {
            _dao = dao;
        }


        public IDictionary<string, List<string>> DoValidateIdsThatHaveWorkorders(string baseQuery, string itemid, ICollection<AssociationOption> items, string classification) {
            var sb = new StringBuilder();
            sb.AppendFormat(BaseBatchLocationQuery, BaseQueryUtil.GenerateInString(items.Select(i => i.Value)));
            if (classification != null) {
                sb.AppendFormat(" and classstructureid = '{0}'", classification);
            }
            var queryResult = _dao.FindByNativeQuery(sb.ToString());

            var result = new Dictionary<string, List<string>>();

            foreach (var row in queryResult) {
                var itemId = row[itemid];
                var wonum = row["wonum"];
                var summary = row["description"];
                if (!result.ContainsKey(itemId)) {
                    result.Add(itemId, new List<string> { wonum });
                } else {
                    result[itemId].Add(wonum);
                }
            }
            return result;
        }


        [NotNull]
        public IDictionary<string, List<string>> ValidateIdsThatHaveWorkordersForLocation(ICollection<AssociationOption> items, string classification) {
            return DoValidateIdsThatHaveWorkorders(BaseBatchLocationQuery, "location", items, classification);
        }

      
        [NotNull]
        public IDictionary<string, List<string>> ValidateIdsThatHaveWorkordersForAsset(ICollection<AssociationOption> items, string classification) {
            return DoValidateIdsThatHaveWorkorders(BaseAssetQuery, "assetnum", items, classification);
        }

        public ApplicationListResult GetRelatedLocationWorkorders(string location, string classification) {
            var sb = new StringBuilder();
            sb.AppendFormat(BaseSingleLocationWhereQuery, location);
            if (classification != null) {
                sb.AppendFormat("and classstructureid = {0}", classification);
            }
            var queryResult = _dao.FindByNativeQuery(BaseSingleLocationProjectionQuery +sb);

            var dataSet =DataSetProvider.GetInstance().LookupDataSet("workorder", "relatedwoconfirmationlist");
            var app =MetadataProvider.Application("workorder").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("relatedwoconfirmationlist"));
            PaginatedSearchRequestDto dto = PaginatedSearchRequestDto.DefaultInstance(app.Schema);
            dto.PageSize = 10;
            dto.FilterFixedWhereClause = sb.ToString();
            return dataSet.GetList(app, dto);
        }



    }
}
