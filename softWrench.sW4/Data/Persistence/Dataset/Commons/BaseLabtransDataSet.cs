using System;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softWrench.sW4.Security.Services;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class BaseLabtransDataSet : MaximoApplicationDataSet {

        public override async Task<TargetResult> Execute(ApplicationMetadata application, JObject json, string id, string operation, bool isBatch, Tuple<string, string> userIdSite) {

            TargetResult result;

            // Make sure that premium hours is not sumitted as an empty string
            var payhours = json.GetValue("premiumpayhours");
            json["premiumpayhours"] = payhours.ToString() == "" ? null : payhours;

            if (application.Schema.SchemaId.EqualsIc("editdetail")) {
                // Remove the current id
                var labtransId = json.GetValue("labtransid");
                json.Remove("labtransid");
                operation = "crud_create";
                // Submit to create the new lab trans
                result = await base.Execute(application, json, null, operation, isBatch, null);
                result.ResultObject = null;
                result.SuccessMessage = "Labor successfully updated";
                // Delete the original if it was an edit
                MaximoHibernateDAO.GetInstance()
                    .ExecuteSql("delete from labtrans where labtransid = ? ", labtransId.ToString());
            } else {
                result = await base.Execute(application, json, id, operation, isBatch, userIdSite);
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
