using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using NHibernate.Util;
using softwrench.sw4.problem.classes;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Dataset.Commons.SWDB;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.chicago.classes.com.cts.chicago.dataset {
    public class ChicagoProblemDataSet : BaseProblemDataSet {

        private const string IsmIdQuery = "select ticketid, ismticketid from sr where ticketid in (:p0)";

        private readonly IMaximoHibernateDAO _hibernateDAO;

        public ChicagoProblemDataSet(ProblemHandlerLookuper handlerLookuper, DataSetProvider dataSetProvider, IMaximoHibernateDAO hibernateDAO) : base(handlerLookuper, dataSetProvider) {
            _hibernateDAO = hibernateDAO;
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = await base.GetList(application, searchDto);
            var list = result.ResultObject.ToList();
            if (!list.Any()) {
                return result;
            }

            var ids = new List<string>();
            var srList = list.Where(IsServiceRequest).ToList();
            if (!srList.Any()) {
                return result;
            }

            srList.ForEach(row => {
                var recordUserId = row.GetAttribute("recorduserid") as string;
                if (!string.IsNullOrEmpty(recordUserId)) {
                    ids.Add(recordUserId);
                }
            });

            var ismIdsResult = _hibernateDAO.FindByNativeQuery(IsmIdQuery, ids);
            if (!ismIdsResult.Any()) {
                return result;
            }

            ismIdsResult.ForEach(dict => {
                string ticketId, ismTickedId;
                dict.TryGetValue("ticketid", out ticketId);
                dict.TryGetValue("ismticketid", out ismTickedId);
                if (string.IsNullOrEmpty(ticketId) || string.IsNullOrEmpty(ismTickedId)) {
                    return;
                }
                AddIsmId(ticketId, ismTickedId, srList);
            });

            return result;
        }

        private static bool IsServiceRequest(AttributeHolder row) {
            object type;
            row.TryGetValue("recordtype", out type);
            if (type == null) {
                return false;
            }

            var typeString = type as string;
            return typeString != null && ("servicerequest".Equals(typeString) || "quickservicerequest".Equals(typeString));
        }

        private static void AddIsmId(string ticketId, string ismTickedId, IEnumerable<AttributeHolder> srList) {
            var doBreak = false;
            srList.ForEach(row => {
                if (doBreak) {
                    return;
                }
                var recordUserId = row.GetAttribute("recorduserid") as string;
                if (string.IsNullOrEmpty(recordUserId) || !ticketId.Equals(recordUserId)) {
                    return;
                }
                row.Add("ismticketid", ismTickedId);
                doBreak = true;
            });
        }

        public override string ApplicationName() {
            return "_SoftwrenchError";
        }

        public override string ClientFilter() {
            return "chicago";
        }
    }
}
