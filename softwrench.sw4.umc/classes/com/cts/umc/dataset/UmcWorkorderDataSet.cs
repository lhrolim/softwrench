using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Util;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket.ServiceRequest;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.umc.classes.com.cts.umc.dataset {
    public class UmcWorkorderDataSet : BaseWorkorderDataSet {

        public UmcWorkorderDataSet(ISWDBHibernateDAO swdbDao) : base(swdbDao) {

        }

        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var result = await base.GetApplicationDetail(application, user, request);
            if (!request.IsEditionRequest) {
                //https://controltechnologysolutions.atlassian.net/browse/SWWEB-3377
                var dm =result.ResultObject;
                dm.SetAttribute("worktype","RO");
                dm.SetAttribute("classstructureid", "1005");
                dm.SetAttribute("wopriority", "3");
            }
            return result;
        }


        protected override async Task LazyPopulateFailureCodes(ApplicationDetailResult baseDetail, object siteid) {
            var items = await MaxDAO.FindByNativeQueryAsync(
                "select r.failurecode as failurecode,linenum,type,f.description as description from failurereport r inner join failurecode f on (r.failurecode = f.failurecode and r.orgid = f.orgid) where wonum = ? and siteid=?",
                baseDetail.ResultObject["wonum"], siteid);
            foreach (var item in items) {
                var type = item["type"];
                var projectedFieldName = "";
                var descriptionName = "";
                if (type.EqualsIc("PROBLEM")) {
                    descriptionName = "#problemlist_.description";
                    projectedFieldName = "#problemlist_.failurelist";
                } else if (type.EqualsIc("CAUSE")) {
                    projectedFieldName = "#causelist_.failurelist";
                    descriptionName = "#causelist_.description";
                    baseDetail.ResultObject.SetAttribute("fr1code", item["failurecode"]);
                } else {
                    projectedFieldName = "#remedylist_.failurelist";
                    descriptionName = "#remedylist_.description";
                    baseDetail.ResultObject.SetAttribute("fr2code", item["failurecode"]);
                }
                baseDetail.ResultObject.SetAttribute(projectedFieldName, item["linenum"]);
                baseDetail.ResultObject.SetAttribute(descriptionName, item["description"]);
            }
        }


        public override string ApplicationName() {
            return "workorder";
        }

        public override string ClientFilter() {
            return "umc";
        }

    }
}
