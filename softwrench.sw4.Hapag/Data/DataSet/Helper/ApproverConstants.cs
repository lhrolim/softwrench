
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {
    class ApproverConstants {

        public const string ChangeOpenApprovalsDashQuery = @"
        wochange.status = 'AUTH'
        and dashapprovals_.wonum is not null 
        and not exists (
		    select 1 from worklog apprwo where apprwo.class = 'CHANGE' and apprwo.RECORDKEY = wochange.WONUM
	        and apprwo.ITDCREATEDATE > wochange.statusdate
	        and ( (apprwo.logtype = 'APPROVAL OBTAINED' and DESCRIPTION in ('Approved by group ' || dashapprovals_.approvergroup)  )
                or
                (apprwo.logtype = 'REASON REJECTING' and DESCRIPTION like ('Rejected by group%')  )))
	    ";


        public const string ChangeByColumn = "#changeby";
        public const string ChangeDateColumn = "#changedate";
        public const string StatusColumn = "#status";
        public const string ActionColumn = "action";
        public const string WfIdColumn = "wfid";
        public const string RoleIdColumn = "roleid";


        //WFTRANSACTION
        public const string TransDateColumn = "transdate";
        public const string TransTypeColumn = "transtype";
        public const string PersonIdColumn = "personid";

        //Worklogs
        public const string CreateByColumn = "createby";
        public const string CreateDate = "itdcreatedate";

        public const string WlApprLogType = "APPROVAL OBTAINED";
        public const string WlRejLogType = "REASON REJECTING";


        public const string ApproverGroupColumn = "approvergroup";
        public const string ChangeLevel = "PMCHGAPPROVALLEVEL";
        public const string ApprovalLevel = "APPROVALLEVEL";

        public const string ApprovedStatus = "Approved";
        public const string RejectedStatus = "Rejected";
        public const string RejectedWorklogDescription = "Rejected by group";




        public static string GetWorkLogDescriptions(IEnumerable<PersonGroupAssociation> personGroups, bool approval) {
            var sb = new StringBuilder();
            var prefix = approval ? "'Approved by group {0}'," : "'Rejected by group {0}',";
            foreach (var group in personGroups) {
                sb.Append(prefix.Fmt(group.GroupName));
            }

            if (sb.Length > 0) {
                return sb.ToString(0, sb.Length - 1);
            } else {
                return "''";
            }
        }

        public static string GetRejectDescription() {
            return "'"+ RejectedWorklogDescription + "%" + "'";
        }

        public static string GetWorkLogDescriptions(string groupName, bool approval) {
            var prefix = approval ? "Approved by group {0}" : "Rejected by group {0}";
            return prefix.Fmt(groupName);

        }

    }
}
