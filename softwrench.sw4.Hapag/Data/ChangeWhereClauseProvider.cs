﻿using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System;

namespace softwrench.sw4.Hapag.Data {
    public class ChangeWhereClauseProvider : ISingletonComponent {

        private readonly IContextLookuper _contextLookuper;

        public ChangeWhereClauseProvider(IContextLookuper contextLookuper) {
            _contextLookuper = contextLookuper;
        }

        internal const string ChangeTemplateId = @"change.templateid IN ({0})";

        internal const string ChangeOpenTasks = @"woactivity.status = 'INPRG' AND woactivity.ownergroup IN ({0}) AND wochange_.wonum is not null";

        internal const string ChangeOpenTasksViewAll = @"exists(select 1 from WOACTIVITY as woactivity_ where wochange.wonum = woactivity_.parent and woactivity_.status = 'INPRG' AND woactivity_.ownergroup IN ({0}))";

        public String ChangeGridQuery() {
            var user = SecurityFacade.CurrentUser();
            return String.Format(@"
            exists (select 1 from pmchgotherapprovers approvals_ where (wochange.wonum = approvals_.wonum) and approvals_.approvergroup IN ({0}) ) 
            or exists (select 1 from WOACTIVITY as woactivity_ where wochange.wonum = woactivity_.parent and woactivity_.ownergroup in ({0}) )"
                , user.GetPersonGroupsForQuery());
        }

        //used on the sr union schema
        public String ChangeSRUnionGridQuery() {
            var templateIds = GetTemplateIds();
            return @"not exists (select 1 from wochange wo4sr_ where wo4sr_.origrecordid = srforchange.ticketid and wo4sr_.origrecordclass = 'SR' and wo4sr_.woclass = 'CHANGE')
                and srforchange.templateid in ({0})".Fmt(templateIds);
        }

        public string DashboardChangeTasksWhereClause() {
            var user = SecurityFacade.CurrentUser();
            return ChangeOpenTasks.Fmt(user.GetPersonGroupsForQuery());
        }

        public string DashboardChangeTasksWhereClauseViewAll() {
            var user = SecurityFacade.CurrentUser();
            return ChangeOpenTasksViewAll.Fmt(user.GetPersonGroupsForQuery());
        }

        public string GetWorkLogJoinQuery() {
            return
                @"(dashapprovals_apprwo_.logtype = 'APPROVAL OBTAINED' and dashapprovals_apprwo_.DESCRIPTION = 'Approved by group ' 
                || dashapprovals_.approvergroup )";
        }

        public string GetPersonGroupsForQuery() {
            var user = SecurityFacade.CurrentUser();
            var groups = user.GetPersonGroupsForQuery();
            return @"dashapprovals_.approvergroup IN ({0})".Fmt(groups);
        }

        public string DashboardChangeApprovalsWhereClause() {
            var user = SecurityFacade.CurrentUser();
            var workLogDescriptions = ApproverConstants.GetWorkLogDescriptions(user.PersonGroups, false);
            return ApproverConstants.ChangeOpenApprovalsDashQuery.Fmt(workLogDescriptions);
        }


        private string GetTemplateIds() {
            var module = _contextLookuper.LookupContext().Module;
            if (module.Equals(FunctionalRole.Sso.ToString(), StringComparison.InvariantCultureIgnoreCase)) {
                return "'HLCDECHSSO'";
            }
            if (module.Equals(FunctionalRole.Tui.ToString(), StringComparison.InvariantCultureIgnoreCase)) {
                return "'HLCDECHTUI'";
            }
            return "'HLCDECHG','HLCDECHTUI','HLCDECHSSO'";
        }
    }
}
