using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.web.Util;
using JetBrains.Annotations;
using log4net;
using Microsoft.Ajax.Utilities;
using Quartz.Util;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Association.SchemaLoading;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Entities.Workflow.DTO;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Entities.Workflow {
    public class MaximoWorkflowManager : ISingletonComponent {

        private readonly IMaximoHibernateDAO _maxDAO;
        private readonly MaximoConnectorEngine _maximoConnectorEngine;

        private readonly ApplicationSchemaDefinition _cachedActionModalSchema;
        private readonly IDictionary<string, ApplicationMetadata> _cachedWorkorderSchemas = new Dictionary<string, ApplicationMetadata>();

        private ILog Log = LogManager.GetLogger(typeof(MaximoWorkflowManager));


        public MaximoWorkflowManager(IMaximoHibernateDAO maximoHibernateDAO, MaximoConnectorEngine maximoConnectorEngine) {
            _maxDAO = maximoHibernateDAO;
            _maximoConnectorEngine = maximoConnectorEngine;
            if (MetadataProvider.IsApplicationEnabled("workflow")) {
                _cachedActionModalSchema =
                    MetadataProvider.Application("workflow").Schema(new ApplicationMetadataSchemaKey("workflowRouting"));
            }

            if (MetadataProvider.IsApplicationEnabled("workorder")) {
                var completeWO = MetadataProvider.Application("workorder");
                _cachedWorkorderSchemas.Add("workorder", completeWO.StaticFromSchema("editdetail"));
            }
        }


        // {0} - workflowName, {1} - entity, {2} - key attribute, {3} - siteid
        private const string RequestTemplate = @"<Initiate{0} xmlns='http://www.ibm.com/maximo'>
                                                   <{1}MboKey>
                                                     <{1}>
                                                       {2}
                                                       <SITEID>{3}</SITEID>
                                                     </{1}>
                                                   </{1}MboKey>
                                                 </Initiate{0}>";


        private const string WfQueryString = "select wfprocessid, processname from wfprocess p where active = 1 and enabled = 1 and {0} = '{1}' and not exists(select 1 from wfinstance i where i.processname = p.processname and i.active = 1 and i.ownertable = '{2}' and i.ownerid = '{3}')";

        private const string ActiveInstancesQuery = "select wfid,processname from wfinstance where ownertable = ? and ownerid = ? and active = 1";

        private const string ActiveInstanceQuery = "select wfid,processname from wfinstance where ownertable = ? and ownerid = ? and processname = ? and active = 1";

        //        private const string WfAssignmentsQuery =
        //
        //        @"select wf.assignid, wf.processname from wfassignment wf
        //        where ownertable = ? and ownerid = ?
        //        and wf.assignstatus in (select value from synonymdomain where domainid='WFASGNSTATUS' and maxvalue='ACTIVE')
        //        and (wf.origperson = ?  or exists (
        //        select 1 from maxrole m where wf.roleid = m.maxrole and type = 'PERSON' and value = ?
        //        union all
        //        select 1 from maxrole m2 
        //        inner join persongroupview pv
        //        on m2.value = pv.persongroup
        //        where wf.roleid = m2.maxrole and m2.type = 'PERSONGROUP' 
        //        and pv.personid = ?
        //        ))";

        private const string WfAssignmentsQuery =

       @"select wf.assignid, wf.processname from wfassignment wf
        where ownertable = ? and ownerid = ?
        and wf.assignstatus in (select value from synonymdomain where domainid='WFASGNSTATUS' and maxvalue='ACTIVE')
        and (wf.assigncode = ?) ";



        //        private const string WfActionsQuery =
        //        @"select wf.description, wfa.actionid, wfa.instruction, wf.wfid, wf.processname, wf.assignid from wfassignment wf
        //        inner join wfaction wfa
        //        on (wf.nodeid = wfa.ownernodeid and wfa.processname = wf.processname and wf.processrev = wfa.processrev)
        //        inner join wfcallstack c on (wf.nodeid = c.nodeid and wf.wfid = c.wfid)
        //        where wf.assignid = ?
        //        and wf.assignstatus in (select value from synonymdomain where domainid='WFASGNSTATUS' and maxvalue='ACTIVE')
        //        and (wf.origperson = ? or exists (
        //        select 1 from maxrole m where wf.roleid = m.maxrole and type = 'PERSON' and value = ?
        //        union all
        //        select 1 from maxrole m2 
        //        inner join persongroupview pv
        //        on m2.value = pv.persongroup
        //        where wf.roleid = m2.maxrole and m2.type = 'PERSONGROUP' 
        //        and pv.personid = ?
        //        ))";

        private const string WfActionsQuery =
        @"select wf.description, wfa.actionid, wfa.instruction, wf.wfid, wf.processname, wf.assignid from wfassignment wf
        inner join wfaction wfa
        on (wf.nodeid = wfa.ownernodeid and wfa.processname = wf.processname and wf.processrev = wfa.processrev)
        inner join wfcallstack c on (wf.nodeid = c.nodeid and wf.wfid = c.wfid)
        where wf.assignid = ?
        and wf.assignstatus in (select value from synonymdomain where domainid='WFASGNSTATUS' and maxvalue='ACTIVE')
        and (wf.assigncode = ? )";

        private const string ClosedStatusQuery = "select s.description from synonymdomain s inner join {0} w on w.status = s.value where {1} = '{2}' and domainid = 'WOSTATUS' and maxvalue in ('CLOSE', 'CAN')";


        [NotNull]
        public IList<AssociationOption> LocateAssignmentsToRoute(string entityName, string entityId, InMemoryUser user) {
            var results = _maxDAO.FindByNativeQuery(WfAssignmentsQuery, entityName, entityId, user.MaximoPersonId);
            return results.Select(r => new AssociationOption(r["assignid"], r["processname"])).ToList();
        }
        /// <summary>
        ///  Retrieves a list of workflows which are not yet active
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="workflowName"></param>
        /// <param name="appId">the id of the entity such as workorderid, ticketuid, etc</param>
        /// <returns>a dict with wfid,processname</returns>
        public List<Dictionary<string, string>> GetAvailableWorkflows(string appName, string workflowName, string appId) {

            var entityName = _cachedWorkorderSchemas[appName].Schema.EntityName;


            var queryString = workflowName != null
                ? Quartz.Util.StringExtensions.FormatInvariant(WfQueryString, workflowName, entityName, appId)
                : Quartz.Util.StringExtensions.FormatInvariant(WfQueryString, "objectname", entityName, entityName, appId);

            return _maxDAO.FindByNativeQuery(queryString);
        }

        public List<Dictionary<string, string>> GetListOfActiveWorkflows(string entityName, string id) {
            var workflows = _maxDAO.FindByNativeQuery(ActiveInstancesQuery, entityName, id);
            return workflows;
        }

        public Dictionary<string, string> GetActiveWorkflow(string entityName, string id, string processName) {
            var workflows = _maxDAO.FindByNativeQuery(ActiveInstanceQuery, entityName, id, processName);
            return workflows.FirstOrDefault();
        }

        [NotNull]
        public ApplicationDetailResult LocateWfActionsToRoute(string wfAssignmentId, InMemoryUser user) {
            var results = _maxDAO.FindByNativeQuery(WfActionsQuery, wfAssignmentId, user.MaximoPersonId);
            var taskOptions = results.Select(r => new AssociationOption(r["actionid"], r["instruction"])).ToList();
            var tasklabel = results[0]["description"];
            var wfid = results[0]["wfid"];
            var processName = results[0]["processname"];
            var assignid = results[0]["assignid"];
            var datamap = DataMap.GetInstanceFromStringDictionary("workflow", new Dictionary<string, string>{
                {"#tasklabel", tasklabel},
                {"#wfid", wfid},
                {"#processname", processName},
                {"#wfassignmentid", assignid},
            });

            var schemaResult = new AssociationMainSchemaLoadResult {
                EagerOptions = new Dictionary<string, IEnumerable<IAssociationOption>>()
            };
            schemaResult.EagerOptions["#taskoptions"] = taskOptions;

            return new ApplicationDetailResult(datamap, schemaResult, _cachedActionModalSchema, null, null);

        }

        public IGenericResponseResult DoInitWorkflow(string appId, string appName, string appUserId, string siteid, string orgId, List<Dictionary<string, string>> workflows) {
            var appMetadata = _cachedWorkorderSchemas[appName];
            var entityName = appMetadata.Schema.EntityName;

            var entityMetadata = MetadataProvider.EntityByApplication(entityName);

            var workflow = workflows[0];
            string workflowName = workflow["processname"];

            var personId = SecurityFacade.CurrentUser().MaximoPersonId;


            IDictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add("wonum", appUserId);
            attributes.Add("siteid", siteid);
            attributes.Add("orgid", orgId);
            attributes.Add("workflowinfo", personId + ";start;" + workflowName + ";;;");

            _maximoConnectorEngine.Update(new CrudOperationData(appId, attributes, new Dictionary<string, object>(), entityMetadata, appMetadata));

            //            var baseUri = ApplicationConfiguration.WfUrl;
            //            var requestUri = baseUri + workflowName;
            //            var msg = RequestTemplate.FormatInvariant(workflowName.ToUpper(), entityName.ToUpper(),
            //                BuildKeyAttributeString(entityName, appUserId), siteid, personId);
            //
            //            await RestUtil.CallRestApi(requestUri, "POST", null, msg);
            var successMessage = "Workflow {0} has been initiated.".Fmt(workflowName);
            return new BlankApplicationResponse {
                SuccessMessage = successMessage
            };
        }



        public BlankApplicationResponse ValidateCloseStatus(string appName, string appid, bool initingWorkflow) {
            var appMetadata = _cachedWorkorderSchemas[appName];
            var entityName = appMetadata.Schema.EntityName;

            var results = _maxDAO.FindByNativeQuery(ClosedStatusQuery.Fmt(entityName, appMetadata.Schema.IdFieldName, appid));
            if (results.Any()) {
                return new BlankApplicationResponse() {
                    ErrorMessage = "Cannot {2} a workflow for a {0} of status {1}".Fmt(appMetadata.Title, results[0]["description"], initingWorkflow ? "Initialize" : "Route")
                };
            }
            return null;
        }

        public IGenericResponseResult DoRouteWorkFlow(RouteWorkflowDTO routeWorkflowDTO) {
            var appMetadata = _cachedWorkorderSchemas[routeWorkflowDTO.OwnerTable];
            var entityMetadata = MetadataProvider.EntityByApplication(routeWorkflowDTO.OwnerTable);

            //TODO: make it workorder agnostic



            IDictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add("wonum", routeWorkflowDTO.AppUserId);
            attributes.Add("siteid", routeWorkflowDTO.SiteId);
            attributes.Add("orgid", routeWorkflowDTO.OrgId);
            attributes.Add("workflowinfo", BuildWorkflowInfo(routeWorkflowDTO));

            Log.DebugFormat("routing workflow for workorder  {0}", routeWorkflowDTO.AppUserId);

            _maximoConnectorEngine.Update(new CrudOperationData(routeWorkflowDTO.OwnerId, attributes, new Dictionary<string, object>(), entityMetadata,
                appMetadata));



            return new BlankApplicationResponse() {
                SuccessMessage = "Workflow {0} routed successfully".Fmt(routeWorkflowDTO.ProcessName)
            };
        }

        //protocol: personid;actiontype;wfid;wfactionid;wfassignmentid;memo;
        private static string BuildWorkflowInfo(RouteWorkflowDTO routeWorkflowDTO) {
            return SecurityFacade.CurrentUser().MaximoPersonId + ";route;" + routeWorkflowDTO.WfId + ";" + routeWorkflowDTO.ActionId + ";" + routeWorkflowDTO.AssignmentId + ";" + routeWorkflowDTO.Memo;
        }

        private string BuildKeyAttributeString(string entityName, string applicationItemId) {
            string keyTemplate = "<{0}>{1}</{0}>";
            EntityMetadata entity = MetadataProvider.Entity(entityName);
            var formattedKey = Quartz.Util.StringExtensions.FormatInvariant(keyTemplate, entity.UserIdFieldName.ToUpper(), applicationItemId);
            return formattedKey;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="id">id of the entity, such as workorderid, ticketuid</param>
        /// <param name="userId">userid of the entity, such as wonum, ticketid</param>
        /// <param name="siteid"></param>
        /// <param name="orgid"></param>
        /// <param name="workflow"> a dictionary with wfid and processname</param>
        /// <returns></returns>
        public IGenericResponseResult DoStopWorkFlow(string entityName, string id, string userId, string siteid, string orgid, Dictionary<string, string> workflow) {
            var appMetadata = _cachedWorkorderSchemas[entityName];

            var entityMetadata = MetadataProvider.Entity(entityName);

            IDictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add(appMetadata.Schema.UserIdFieldName, userId);
            attributes.Add("siteid", siteid);
            attributes.Add("orgid", orgid);

            attributes.Add("workflowinfo", SecurityFacade.CurrentUser().MaximoPersonId + ";stop;" + workflow["wfid"] + ";;;");

            _maximoConnectorEngine.Update(new CrudOperationData(id, attributes, new Dictionary<string, object>(), entityMetadata,
                appMetadata));


            return new BlankApplicationResponse() {
                SuccessMessage = "Workflow {0} stopped successfully".Fmt(workflow["processname"])
            };
        }

    }
}
