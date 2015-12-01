using System;
using System.Collections.Generic;
using System.Web.Http;
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
                List<Dictionary<string, string>> workflows =
                    _maximoDao.FindByNativeQuery(
                        "select * from wfprocess where active = 1 and enabled = 1 and objectname = '{0}'"
                            .FormatInvariant(entity));
                if (workflows.Count < 1) {
                    return;
                }
                workflow = workflows[0];
            } else {
                var result =
                    _maximoDao.FindSingleByNativeQuery<Dictionary<string, string>>(
                        "select * from wfprocess where active = 1 and enabled = 1 and processname = '{0}'".FormatInvariant(workflowName));
            }

            var baseUrl = ApplicationConfiguration.WfUrl;
            // URL
            var url = baseUrl + workflowName;
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
        }
    }
}
