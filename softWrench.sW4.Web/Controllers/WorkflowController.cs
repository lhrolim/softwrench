using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

namespace softWrench.sW4.Web.Controllers {
    public class WorkflowController : ApiController {
        private readonly MaximoHibernateDAO _maximoDao;

        public WorkflowController(MaximoHibernateDAO dao) {
            _maximoDao = dao;
        }

        [HttpPost]
        public IGenericResponseResult InitiateWorkflow(string entityName, string applicationItemId, string siteid, string workflowName) {
            List<Dictionary<string, string>> workflows;
            if (workflowName == null) {
                workflows = _maximoDao.FindByNativeQuery("select wfprocessid, processname from wfprocess where active = 1 and enabled = 1 and objectname = '{0}'".FormatInvariant(entityName));
            } else {
                workflows = _maximoDao.FindByNativeQuery("select wfprocessid, processname from wfprocess where active = 1 and enabled = 1 and processname = '{0}'".FormatInvariant(workflowName));
            }
            // If there are no work flows
            if (!workflows.Any()) {
                // Return some type of error response if there are no workflows?
                return null;
            }
            // If there are multiple work flows
            if (workflows.Count > 1) {
                var workflowSchema = MetadataProvider.Application("workflow").Schema(new ApplicationMetadataSchemaKey("workflowselection"));
                IList<IAssociationOption> workflowOptions = workflows.Select(w => new GenericAssociationOption(w["processname"], w["processname"])).Cast<IAssociationOption>().ToList();
                var dto = new WorkflowDTO() {
                    Workflows = workflowOptions,
                    Schema = workflowSchema
                };
                return new GenericResponseResult<WorkflowDTO>(dto);
            }
            var workflow = workflows[0];
            workflowName = workflow["processname"];
            var baseUri = ApplicationConfiguration.WfUrl;
            var requestUri = baseUri + workflowName;
            // {0} - workflowName, {1} - entity, {2} - key attribute, {3} - siteid
            var requestTemplate = @"<Initiate{0} xmlns='http://www.ibm.com/maximo'>
                                      <{1}MboKey>
                                        <{1}>
                                          {2}
                                          <SITEID>{3}</SITEID>
                                        </{1}>
                                      </{1}MboKey>
                                    </Initiate{0}>";
            var msg = requestTemplate.FormatInvariant(workflowName.ToUpper(), entityName.ToUpper(), BuildKeyAttributeString(entityName, applicationItemId), siteid);
            var response = CallRestApi(requestUri, "POST", null, msg);
            var successMessage = "Workflow {0} has been initiated.".FormatInvariant(workflowName);
            return new GenericResponseResult<HttpWebResponse>(response, successMessage);
        }

        private HttpWebResponse CallRestApi(string url, string method, Dictionary<string, string> headers = null, string payload = null) {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            // headers
            if (headers != null) {
                headers.ForEach((e) => request.Headers[e.Key] = e.Value);
            }
            request.ContentType = "application/xml";
            // write payload to requests stream
            if (!string.IsNullOrEmpty(payload)) {
                var body = Encoding.UTF8.GetBytes(payload);
                request.ContentLength = body.Length;
                using (var requestStream = request.GetRequestStream()) {
                    requestStream.Write(body, 0, body.Length);
                }
            }
            // fetch response
            return (HttpWebResponse)request.GetResponse();
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
