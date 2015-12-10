using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows.Input;
using DocumentFormat.OpenXml.Spreadsheet;
using NHibernate.Linq;
using NHibernate.Util;
using Quartz.Util;
using cts.commons.web.Attributes;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.Controllers {
    public class WorkflowController : ApiController {
        private readonly MaximoHibernateDAO _maximoDao;
        // {0} - workflowName, {1} - entity, {2} - key attribute, {3} - siteid
        private const string RequestTemplate = @"<Initiate{0} xmlns='http://www.ibm.com/maximo'>
                                                   <{1}MboKey>
                                                     <{1}>
                                                       {2}
                                                       <SITEID>{3}</SITEID>
                                                     </{1}>
                                                   </{1}MboKey>
                                                 </Initiate{0}>";
        private const string WFQueryString = "select wfprocessid, processname from wfprocess where active = 1 and enabled = 1 and {0} = '{1}'";
        readonly ApplicationSchemaDefinition _workflowSchema;

        public WorkflowController(MaximoHibernateDAO dao) {
            _maximoDao = dao;
            _workflowSchema = MetadataProvider.Application("workflow").Schema(new ApplicationMetadataSchemaKey("workflowselection"));
        }

        [HttpPost]
        public async Task<IGenericResponseResult> InitiateWorkflow(string entityName, string applicationItemId, string siteid, string workflowName) {
            List<Dictionary<string, string>> workflows;
            var queryString = workflowName != null
                ? WFQueryString.FormatInvariant("processname", workflowName)
                : WFQueryString.FormatInvariant("objectname", entityName);
            workflows = _maximoDao.FindByNativeQuery(queryString);
            // If there are no work flows
            if (!workflows.Any()) {
                // Returning null will pop a warning message on the client side
                return null;
            }
            // If there are multiple work flows
            if (workflows.Count > 1) {
                IList<IAssociationOption> workflowOptions = workflows.Select(w => new GenericAssociationOption(w["processname"], w["processname"])).Cast<IAssociationOption>().ToList();
                var dto = new WorkflowDTO() {
                    Workflows = workflowOptions,
                    Schema = _workflowSchema
                };
                return new GenericResponseResult<WorkflowDTO>(dto);
            }
            var workflow = workflows[0];
            workflowName = workflow["processname"];
            var baseUri = ApplicationConfiguration.WfUrl;
            var requestUri = baseUri + workflowName;
            var msg = RequestTemplate.FormatInvariant(workflowName.ToUpper(), entityName.ToUpper(), BuildKeyAttributeString(entityName, applicationItemId), siteid);
            var response = await RestUtil.CallRestApi(requestUri, "POST", null, msg);
            var successMessage = "Workflow {0} has been initiated.".FormatInvariant(workflowName);
            return new GenericResponseResult<WebResponse>(response, successMessage);
        }

        private string BuildKeyAttributeString(string entityName, string applicationItemId) {
            string keyTemplate = "<{0}>{1}</{0}>";
            EntityMetadata Entity = MetadataProvider.Entity(entityName);
            var formattedKey = keyTemplate.FormatInvariant(Entity.UserIdFieldName.ToUpper(), applicationItemId);
            return formattedKey;
        }
    }

    public class WorkflowDTO {
        public ApplicationSchemaDefinition Schema {
            get; set;
        }

        public IEnumerable<IAssociationOption> Workflows {
            get; set;
        }
    }
}
