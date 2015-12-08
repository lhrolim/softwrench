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
        public IGenericResponseResult InitiateWorkflow(string entity, string schema, string applicationItemId, string workflowName) {
            List<Dictionary<string, string>> workflows = null;
            Dictionary<string, string> workflow;
            if (workflowName == null) {
                workflows = _maximoDao.FindByNativeQuery("select wfprocessid, processname from wfprocess where active = 1 and enabled = 1 and objectname = '{0}'".FormatInvariant(entity));

            } else {
                workflows = _maximoDao.FindByNativeQuery("select wfprocessid, processname from wfprocess where active = 1 and enabled = 1 and processname = '{0}'".FormatInvariant(workflowName));
            }
            // If there are no work flows
            if (workflows.Count < 1) {
                return null;
            }
            // If there are multiple work flows
            if (workflows.Count > 1) {
                var workflowSchema = MetadataProvider.Application("workflow").Schema(new ApplicationMetadataSchemaKey("workflowselection"));
                IList<IAssociationOption> workflowOptions = workflows.Select(w => new GenericAssociationOption(w["wfprocessid"], w["processname"])).Cast<IAssociationOption>().ToList();
                var dto = new WorkflowDTO() {
                    workflows = workflowOptions,
                    schema = workflowSchema
                };
                return new GenericResponseResult<WorkflowDTO>(dto);
            }
            workflow = workflows[0];
            var baseUrl = ApplicationConfiguration.WfUrl;
            // URL
            var url = baseUrl + workflow["processname"];
            // {0}-workflow, {1}-entity, {2}-attributes
            var template = @"<Initiate{0} xmlns='http://www.ibm.com/maximo'>
                               <{1}MboKey>
                                 <{1}>
                                   {2}
                                 </{1}>
                               </{1}MboKey>
                             </Initiate{0}> ";
            // format the message content
            var msg = template.FormatInvariant(workflowName, entity, BuildAttributesString(entity, schema, applicationItemId));
            // submit the calla nd get response
            var response = CallRestApi(url, "POST", null, msg);
            // return response from initiate
            return new GenericResponseResult<HttpWebResponse>(response);
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

        private string BuildAttributesString(string entity, string schema, string applicationItemId) {
            string keyTemplate = "<{0}>{1}</{0}>";
            EntityMetadata Entity = MetadataProvider.Entity(entity);
            var formattedKey = keyTemplate.FormatInvariant(Entity.UserIdFieldName, applicationItemId);
            return formattedKey;
        }
    }

    public class WorkflowDTO {
        public ApplicationSchemaDefinition schema {
            get; set;
        }

        public IEnumerable<IAssociationOption> workflows {
            get; set;
        }
    }
}
