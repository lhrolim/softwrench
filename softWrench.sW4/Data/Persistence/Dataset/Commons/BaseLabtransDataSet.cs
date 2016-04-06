using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using Quartz.Util;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class BaseLabtransDataSet : MaximoApplicationDataSet {

        public override TargetResult Execute(ApplicationMetadata application, JObject json, string id, string operation, bool isBatch, Tuple<string, string> userIdSite) {

            TargetResult result;
            if (application.Schema.SchemaId.EqualsIc("editdetail")) {
                // Remove the current id
                var labtransId = json.GetValue("labtransid");
                json.Remove("labtransid");
                operation = "crud_create";
                // Submit to create the new lab trans
                result = base.Execute(application, json, null, operation, isBatch, userIdSite);
                // TODO: Due to changing from an update to a create in the backend, there is no redirect and thus the detail data is not retreived by default. Would need a new property to force the retreival of the updated detail on a create call.
                var resultOb = (Array)result.ResultObject;
                var firstOb = resultOb.GetValue(0);
                id = WsUtil.GetRealValue(firstOb, application.IdFieldName).ToString();
                var detailrequest = new DetailRequest(id, application.Schema.GetSchemaKey());
                var detailResult = base.GetApplicationDetail(application, SecurityFacade.CurrentUser(), detailrequest);
                result.ResultObject = detailResult.ResultObject;
                // Delete the original if it was an edit
                MaximoHibernateDAO.GetInstance()
                    .ExecuteSql("delete from labtrans where labtransid = ? ", labtransId.ToString());
            } else {
                result = base.Execute(application, json, id, operation, isBatch, userIdSite);
            }
            
            return result;
        }

        public SearchRequestDto FilterOpenWorkorders(AssociationPreFilterFunctionParameters parameters) {
            var filter = parameters.BASEDto;
            
            filter.AppendWhereClauseFormat("( STATUS IN ('APPR', 'INPRG', 'WMATL', 'WSCH') AND HISTORYFLAG != 1)");

            return filter;
        }

        public override string ApplicationName() {
            return "labtrans";
        }

        public override string ClientFilter() {
            return null;
        }
    }
}
