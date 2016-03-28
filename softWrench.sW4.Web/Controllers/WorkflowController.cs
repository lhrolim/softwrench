using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.portable.Util;
using log4net;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities.Workflow;
using softWrench.sW4.Data.Entities.Workflow.DTO;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Web.Controllers {





    public class WorkflowController : ApiController {

        private static readonly ILog Log = LogManager.GetLogger(typeof(WorkflowController));



        private readonly MaximoWorkflowManager _workflowManager;

        private readonly MaximoHibernateDAO _maximoDao;



        private const string ActiveInstanceQuery = "select wfid,processname from wfinstance where ownertable = ? and ownerid = ? and active = 1";

        private const string WorkFlowById = "select wfid,processname from wfinstance where wfid = ? ";

        readonly ApplicationSchemaDefinition _workflowSchema;

        public WorkflowController(MaximoHibernateDAO dao, MaximoWorkflowManager workflowManager) {
            _maximoDao = dao;
            _workflowManager = workflowManager;
            _workflowSchema = MetadataProvider.Application("workflow").Schema(new ApplicationMetadataSchemaKey("workflowselection"));
        }

        [HttpPost]
        public async Task<IGenericResponseResult> InitiateWorkflow(string appName, string appId, string appUserId, string siteid, string workflowName) {

            var workflows = _workflowManager.GetAvailableWorkflows(appName, workflowName, appId);

            var validationResult = _workflowManager.ValidateCloseStatus(appName, appId);
            if (validationResult != null) {
                return validationResult;
            }

            // If there are no work flows
            if (!workflows.Any()) {
                // Returning null will pop a warning message on the client side
                return new BlankApplicationResponse() {
                    ErrorMessage = "There are no active and enabled Workflows for this record type."
                };
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


            return await _workflowManager.DoInitWorkflow(appId,appName, appUserId, siteid, workflows);
        }


        public IGenericResponseResult StopWorkflow(string entityName, string id, string userId, string siteid, Int32? wfInstanceId) {
            if (wfInstanceId != null) {
                var wfsToStop = _maximoDao.FindByNativeQuery(WorkFlowById, wfInstanceId);
                Log.InfoFormat("Stopping workflow {0} for app {1}", wfInstanceId, entityName);
                if (wfsToStop.Any()) {
                    return _workflowManager.DoStopWorkFlow(entityName, id, userId, siteid, wfsToStop[0]);
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

            return _workflowManager.DoStopWorkFlow(entityName, id, userId, siteid, workflow);
        }

        public IGenericResponseResult InitRouteWorkflow(string entityName, string id, string appuserId, string siteid) {
            var user = SecurityFacade.CurrentUser();
            var assignments = _workflowManager.LocateAssignmentsToRoute(entityName, id, user);
            if (!assignments.Any()) {
                return new BlankApplicationResponse() { ErrorMessage = "There are no active assignments on this workflow for your user" };
            }
            if (assignments.Count() == 1) {
                return _workflowManager.LocateWfActionsToRoute(assignments[0].Value, user);
            }
            var dto = new WorkflowDTO() {
                Workflows = assignments,
                Schema = _workflowSchema
            };
            return new GenericResponseResult<WorkflowDTO>(dto);
        }

        public IGenericResponseResult InitRouteWorkflowSelected(string wfAssignmentId) {
            var user = SecurityFacade.CurrentUser();
            return _workflowManager.LocateWfActionsToRoute(wfAssignmentId, user);
        }

        [HttpPost]
        public IGenericResponseResult DoRouteWorkflow([FromBody]RouteWorkflowDTO routeWorkflowDTO) {
            return _workflowManager.DoRouteWorkFlow(routeWorkflowDTO);
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
