using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows.Input;
using cts.commons.portable.Util;
using DocumentFormat.OpenXml.Spreadsheet;
using NHibernate.Linq;
using NHibernate.Util;
using Quartz.Util;
using cts.commons.web.Attributes;
using log4net;
using log4net.Core;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities.Workflow;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Util;

namespace softWrench.sW4.Web.Controllers {





    public class WorkflowController : ApiController
    {

        private static ILog Log = LogManager.GetLogger(typeof (WorkflowController));


        #region scripts 
        ////considerations:
        //// strings are java.lang.strings, not javascript strings --> do not use ===, and check for java documentation rather than javascript´s

        //function afterMboData(ctx) {

        //    ctx.log("[WorkOrder Workflow]: init script");
        //    var mbo = ctx.getMbo();
        //    var wonum = ctx.getData().getCurrentData("WONUM");


        //    //using random fields wojp1, wojp2 and wojp3 as the holders for workflow logic
        //    var action = ctx.getData().getCurrentData("WOJP1");
        //    var wfId = ctx.getData().getCurrentData("WOJP2");
        //    var wfActionId = ctx.getData().getCurrentData("WOJP3");


        //    if (action == null || wfId == null) {
        //        ctx.log("[WorkOrder Workflow]-- no custom logic to execute (null wojp1 or null wojp2), skippinng");
        //        //no custom action to execute
        //        return;
        //    }


        //    ctx.log("[WorkOrder Workflow]-- Initing action " + action + " on workflow instance " + wfId);

        //    //fetch active workflows of the workorder
        //    var wfInstanceSet = mbo.getMboSet("ACTIVEWORKFLOW");

        //    if (!wfInstanceSet || wfInstanceSet.isEmpty()) {
        //        ctx.log("[WorkOrder Workflow]-- no active workflows found for workorder " + wonum + " skipping execution");
        //        return;
        //    }

        //    //let´s search for an active workflow with the same id as informed
        //    for (var i = 0; i < wfInstanceSet.count(); i++) {
        //        var wfInst = wfInstanceSet.getMbo(i);
        //        var id = wfInst.getUniqueIDValue();
        //        if (id == wfId) {
        //            ctx.log("active workflow found performing action");
        //            if (action == "stop") {
        //                ctx.log("stopping workflow");
        //                wfInst.stopWorkflow("Auto Stop");
        //            } else if (action == "route") {
        //                if (wfActionId == null) {
        //                    ctx.log("[WorkOrder Workflow]-- wojp3 should inform the action to route to");
        //                    return;
        //                }
        //                mbo.setModified(false);
        //                ctx.log("[WorkOrder Workflow]-- Routing workflow " + wfId + " to action " + wfActionId);
        //                wfInst.applyWorkflowAction(wfActionId)
        //                 ctx.log("[WorkOrder Workflow]--  Workflow Routed");
        //                ctx.skipTxn();
        //            }

        //            return;
        //        }



        //    }

        //    ctx.log("[WorkOrder Workflow]-- Active workflow " + wfId + " not found, no action performed ");

        //}

        #endregion


        private readonly MaximoConnectorEngine _maximoConnectorEngine;

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

        private const string ActiveInstanceQuery = "select wfid,processname from wfinstance where ownertable = ? and ownerid = ? and active = 1";

        private const string WorkFlowById = "select wfid,processname from wfinstance where wfid = ? ";



        readonly ApplicationSchemaDefinition _workflowSchema;

        public WorkflowController(MaximoHibernateDAO dao, MaximoConnectorEngine maximoConnectorEngine) {
            _maximoDao = dao;
            _maximoConnectorEngine = maximoConnectorEngine;
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


        public async Task<IGenericResponseResult> StopWorkflow(string entityName, string id, string userId, string siteid, Int32? wfInstanceId) {
            if (wfInstanceId != null) {
                var wfsToStop = _maximoDao.FindByNativeQuery(WorkFlowById, wfInstanceId);
                Log.InfoFormat("Stopping workflow {0} for app {1}",wfInstanceId,entityName);
                if (wfsToStop.Any()) {
                    return DoStopWorkFlow(id, userId, siteid, wfsToStop[0]);
                }
            }


            var workflows = _maximoDao.FindByNativeQuery(ActiveInstanceQuery, entityName, id);
            // If there are no work flows
            if (!workflows.Any()) {
                // Returning null will pop a warning message on the client side
                return new BlankApplicationResponse() { ErrorMessage = "No Active workflow for workorder {0}".Fmt(userId) };
            }
            // If there are multiple work flows
            if (workflows.Count > 1) {
                IList<IAssociationOption> workflowOptions = workflows.Select(w => new GenericAssociationOption(w["wfid"], w["processname"])).Cast<IAssociationOption>().ToList();
                var dto = new WorkflowDTO() {
                    Workflows = workflowOptions,
                    Schema = _workflowSchema
                };
                return new GenericResponseResult<WorkflowDTO>(dto);
            }
            //otherwise, let´s stop the only one found
            var workflow = workflows[0];

            return DoStopWorkFlow(id, userId, siteid, workflow);
        }

        private IGenericResponseResult DoStopWorkFlow(string id, string userId, string siteid, Dictionary<string, string> workflow) {
            var appMetadata =
                MetadataProvider.Application("workorder")
                    .ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("editdetail"));

            var entityMetadata = MetadataProvider.Entity("workorder");

            IDictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add("wonum", userId);
            attributes.Add("siteid", siteid);

            attributes.Add("wojp1", "stop");
            attributes.Add("wojp2", workflow["wfid"]);

            _maximoConnectorEngine.Update(new CrudOperationData(id, attributes, new Dictionary<string, object>(), entityMetadata,
                appMetadata));


            return new BlankApplicationResponse() {
                SuccessMessage = "Workflow {0} stopped successfully".Fmt(workflow["processname"])
            };
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
