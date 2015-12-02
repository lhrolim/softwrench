using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web.Http;
using NHibernate.Linq;
using NHibernate.Util;
using Quartz.Util;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Util;

namespace softWrench.sW4.Web.Controllers {

    public class WorkflowController : ApiController {
        private readonly MaximoHibernateDAO _maximoDao;

        public WorkflowController(MaximoHibernateDAO dao) {
            _maximoDao = dao;
        }

        [HttpPost]
        public void InitiateWorkflow(string entity, string applicationItemId, string workflowName) {
            Dictionary<string, string> workflow;
            if (workflowName == null) {
                List<Dictionary<string, string>> workflows = _maximoDao.FindByNativeQuery("select * from wfprocess where active = 1 and enabled = 1 and objectname = '{0}'".FormatInvariant(entity));
                if (workflows.Count < 1) {
                    return;
                }
                workflow = workflows[0];
            } else {
                List<Dictionary<string, string>> workflows = _maximoDao.FindByNativeQuery("select * from wfprocess where active = 1 and enabled = 1 and processname = '{0}'".FormatInvariant(workflowName));
                if (workflows.Count < 1) {
                    return;
                }
                workflow = workflows[0];
            }

            var baseUrl = ApplicationConfiguration.WfUrl;
            // URL
            var url = baseUrl + workflow["processname"];
            // {0}-workflow, {1}-entity, {2}-attributes
            var template = @"<Initiate{0} xmlns='http://www.ibm.com/maximo'>
                               <{1}MboKey>
                                 <{1}>
                                   <TICKETID>1841</TICKETID> 
                                   <SITEID>BEDFORD</SITEID>
                                 </{1}>
                               </{1}MboKey>
                             </Initiate{0}> ";
            var msg = template.FormatInvariant(workflowName, entity);

            var response = CallRestApi(url, "POST", null, msg);
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
    }
}
