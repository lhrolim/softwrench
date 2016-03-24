using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using Microsoft.Ajax.Utilities;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API.Association.SchemaLoading;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softWrench.sW4.Data.Entities.Workflow {
    public class MaximoWorkflowManager : ISingletonComponent {

        private readonly IMaximoHibernateDAO _maxDAO;
        private readonly MaximoConnectorEngine _maximoConnectorEngine;

        private ApplicationSchemaDefinition _cachedActionModalSchema;


        public MaximoWorkflowManager(IMaximoHibernateDAO maximoHibernateDAO, MaximoConnectorEngine maximoConnectorEngine) {
            _maxDAO = maximoHibernateDAO;
            _maximoConnectorEngine = maximoConnectorEngine;
            if (MetadataProvider.IsApplicationEnabled("workflow")) {
                _cachedActionModalSchema =
                    MetadataProvider.Application("workflow").Schema(new ApplicationMetadataSchemaKey("workflowRouting"));
            }

        }


        private const string WfAssignmentsQuery =

        @"select wf.assignid, wf.processname from wfassignment wf
        where ownertable = ? and ownerid = ?
        and wf.assignstatus in (select value from synonymdomain where domainid='WFASGNSTATUS' and maxvalue='ACTIVE')
        and (wf.origperson = ?  or exists (
        select 1 from maxrole m where wf.roleid = m.maxrole and type = 'PERSON' and value = ?
        union all
        select 1 from maxrole m2 
        inner join persongroupview pv
        on m2.value = pv.persongroup
        where wf.roleid = m2.maxrole and m2.type = 'PERSONGROUP' 
        and pv.personid = ?
        ))";


        private const string WFActionsQuery =
        @"select wf.description, wfa.actionid, wfa.instruction, wf.wfid, wf.processname, wf.assignid from wfassignment wf
        inner join wfaction wfa
        on (wf.nodeid = wfa.ownernodeid and wfa.processname = wf.processname and wf.processrev = wfa.processrev)
        inner join wfcallstack c on (wf.nodeid = c.nodeid and wf.wfid = c.wfid)
        where wf.assignid = ?
        and wf.assignstatus in (select value from synonymdomain where domainid='WFASGNSTATUS' and maxvalue='ACTIVE')
        and (wf.origperson = ? or exists (
        select 1 from maxrole m where wf.roleid = m.maxrole and type = 'PERSON' and value = ?
        union all
        select 1 from maxrole m2 
        inner join persongroupview pv
        on m2.value = pv.persongroup
        where wf.roleid = m2.maxrole and m2.type = 'PERSONGROUP' 
        and pv.personid = ?
        ))";


        [NotNull]
        public IList<AssociationOption> LocateAssignmentsToRoute(string entityName, string entityId, InMemoryUser user) {
            var results = _maxDAO.FindByNativeQuery(WfAssignmentsQuery, entityName, entityId, user.MaximoPersonId,user.MaximoPersonId, user.MaximoPersonId);
            return results.Select(r => new AssociationOption(r["assignid"], r["processname"])).ToList();
        }

        [NotNull]
        public ApplicationDetailResult LocateWfActionsToRoute(string wfAssignmentId, InMemoryUser user) {
            var results = _maxDAO.FindByNativeQuery(WFActionsQuery, wfAssignmentId, user.MaximoPersonId, user.MaximoPersonId, user.MaximoPersonId);
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

        public class RouteWorkflowDTO {
            public string OwnerId {get; set;}
            public string OwnerTable {get; set;}
            public string AppUserId {get; set;}
            public string SiteId {get; set;}
            public string WfId {get; set;}
            public string ProcessName {get; set;}
            public string Memo {get; set;}
            public string ActionId {get; set;}
            public string AssignmentId {get; set;}

        }


        public IGenericResponseResult DoRouteWorkFlow(RouteWorkflowDTO routeWorkflowDTO) {
            var appMetadata =
                MetadataProvider.Application(routeWorkflowDTO.OwnerTable)
                    .ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("editdetail"));

            var entityMetadata = MetadataProvider.EntityByApplication(routeWorkflowDTO.OwnerTable);

            //TODO: make it workorder agnostic

            IDictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add("wonum", routeWorkflowDTO.AppUserId);
            attributes.Add("siteid", routeWorkflowDTO.SiteId);

            attributes.Add("woeq8", "route");
            attributes.Add("woeq4", SecurityFacade.CurrentUser().MaximoPersonId);
            attributes.Add("woeq9", routeWorkflowDTO.WfId);
            attributes.Add("woeq10", routeWorkflowDTO.ActionId);
            attributes.Add("woeq11", routeWorkflowDTO.Memo);
            attributes.Add("woeq12", Int32.Parse(routeWorkflowDTO.AssignmentId));

            _maximoConnectorEngine.Update(new CrudOperationData(routeWorkflowDTO.OwnerId, attributes, new Dictionary<string, object>(), entityMetadata,
                appMetadata));


            return new BlankApplicationResponse() {
                SuccessMessage = "Workflow {0} routed successfully".Fmt(routeWorkflowDTO.ProcessName)
            };
        }

        public IGenericResponseResult DoStopWorkFlow(string id, string userId, string siteid, Dictionary<string, string> workflow) {
            var appMetadata =
                MetadataProvider.Application("workorder")
                    .ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("editdetail"));

            var entityMetadata = MetadataProvider.Entity("workorder");

            IDictionary<string, object> attributes = new Dictionary<string, object>();
            attributes.Add("wonum", userId);
            attributes.Add("siteid", siteid);

            attributes.Add("woeq8", "stop");
            attributes.Add("woeq9", workflow["wfid"]);

            _maximoConnectorEngine.Update(new CrudOperationData(id, attributes, new Dictionary<string, object>(), entityMetadata,
                appMetadata));


            return new BlankApplicationResponse() {
                SuccessMessage = "Workflow {0} stopped successfully".Fmt(workflow["processname"])
            };
        }

    }
}
