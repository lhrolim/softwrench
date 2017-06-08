using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.portable.Util;
using log4net;
using softWrench.sW4.Data.Entities.Workflow;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Applications.Workorder;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.connector {
    public class FirstSolarWorkorderCrudConnector : BaseWorkOrderCrudConnector {

        [Import]
        public IContextLookuper ContextLookuper { get; set; }

        [Import]
        public MaximoHibernateDAO MaxDAO { get; set; }

        [Import]
        public MaximoWorkflowManager WorkflowManager { get; set; }

        private ILog Log = LogManager.GetLogger(typeof(MaximoWorkflowManager));

        public FirstSolarWorkorderCrudConnector() {
            Log.Debug("init..");
        }

        private string BaseAssignmentWorkflowQuery =

        @"select wf.description, wfa.actionid, wfa.instruction, wf.wfid, wf.processname, wf.assignid from wfassignment wf
        inner join wfaction wfa
        on (wf.nodeid = wfa.ownernodeid and wfa.processname = wf.processname and wf.processrev = wfa.processrev)
        inner join wfcallstack c on (wf.nodeid = c.nodeid and wf.wfid = c.wfid)         
        where wf.assignid = (select wf.assignid from wfassignment wf
        where ownertable = 'workorder' and ownerid = '{0}' and processname = 'EPC-WOP'
        and wf.assignstatus in (select value from synonymdomain where domainid='WFASGNSTATUS' and maxvalue='ACTIVE')
        and (wf.assigncode = '{1}'))
        and wf.assignstatus in (select value from synonymdomain where domainid='WFASGNSTATUS' and maxvalue='ACTIVE')
        and (wf.assigncode = '{1}' )
        and wfa.actionid = '759'";

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeCreation(maximoTemplateData);
            if (ContextLookuper.LookupContext().OfflineMode) {

                var user = SecurityFacade.CurrentUser();

                var root = maximoTemplateData.IntegrationObject;
                WsUtil.SetValue(root, "STATUS", "APPR");

                var arr = ReflectionUtil.InstantiateArrayWithBlankElements(root, "ASSIGNMENT", 1);
                var assignment = arr.GetValue(0);

                WsUtil.SetValue(assignment, "LABORCODE", user.GetProperty("laborcode"));
                WsUtil.SetValue(assignment, "SCHEDULEDATE", DateTime.Now.FromServerToRightKind());
                WsUtil.SetValue(assignment, "FINISHDATE", DateTime.Now.AddMonths(2).FromServerToRightKind());
                WsUtil.CopyFromRootEntity(root, assignment, "orgid", user.OrgId);


            }
        }


        protected override bool WorkorderStatusChange(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData crudData, object wo,
            InMemoryUser user) {

            var selectedStatus = crudData.GetAttribute("status");

            if (!ContextLookuper.LookupContext().OfflineMode || !selectedStatus.Equals("COMP")) {
                return base.WorkorderStatusChange(maximoTemplateData, crudData, wo, user);
            }

            var currentUser = SecurityFacade.CurrentUser();

            var workorderId = crudData.Id;
            var siteid = crudData.GetStringAttribute("siteid");
            var orgid = crudData.GetStringAttribute("orgid");
            var wonum = crudData.GetStringAttribute("wonum");

            var data = WorkflowManager.GetActiveWorkflow("workorder", workorderId, "EPC-WOP");


            if (data == null) {
                return base.WorkorderStatusChange(maximoTemplateData, crudData, wo, user);
            }

          

            Log.InfoFormat("routing workflow for workorder {0}", wonum);

            WorkflowManager.DoStopWorkFlow("workorder", workorderId, wonum, siteid, orgid, data);
            return base.WorkorderStatusChange(maximoTemplateData, crudData, wo, user);
        }





        public override string ClientFilter() {
            return "firstsolar";
        }
    }
}
