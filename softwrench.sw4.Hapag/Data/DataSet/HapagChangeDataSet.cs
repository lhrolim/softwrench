using cts.commons.portable.Util;
using log4net;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Data.Init;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using c = softwrench.sw4.Hapag.Data.DataSet.Helper.ApproverConstants;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagChangeDataSet : HapagBaseApplicationDataSet {


        public HapagChangeDataSet(IHlagLocationManager locationManager, EntityRepository entityRepository, MaximoHibernateDAO maxDao) 
            : base(locationManager, entityRepository, maxDao)
        {
        }

        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application,
            InMemoryUser user, DetailRequest request) {
            var result = base.GetApplicationDetail(application, user, request);
            if (request.Id != null) {
                //this means we´re viewing details of a change
                PopulateApprovalsCompositions(result.ResultObject, user);
            }

            return result;
        }

        private void PopulateApprovalsCompositions(DataMap result, InMemoryUser user) {

            var approvals = result.GetAttribute("approvals_") as IEnumerable<IDictionary<string, object>>;
            if (approvals == null) {
                return;
            }
            var worklogs = (IEnumerable<Dictionary<string, object>>)result.GetAttribute("worklog_");
            var wftransactions = (IEnumerable<Dictionary<string, object>>)result.GetAttribute("apprwftransaction_");
            var wfassignment = (IEnumerable<Dictionary<string, object>>)result.GetAttribute("wfassignment_");

            //let´s locate the first one which is marked as reject. 
            //it should always be the latest txlog, since no further operation is allowed after rejecting the workflow
            var rejectedTransaction = wftransactions.FirstOrDefault(w => w[ApproverConstants.TransTypeColumn].ToString().Equals("Reject"));

            foreach (var approval in approvals) {
                var approvalGroup = (String)approval[c.ApproverGroupColumn];
                //non customer approvals
                if (!approvalGroup.StartsWith("C-")) {
                    HandleNonCustomerApprovers(user, wfassignment, approvalGroup, approval, wftransactions, rejectedTransaction);
                } else {
                    HandleCustomerApprovers(result, approvalGroup, worklogs, approval);
                }
            }
        }

        private void HandleNonCustomerApprovers(InMemoryUser user, IEnumerable<Dictionary<string, object>> wfassignment, string approvalGroup,
            IDictionary<string, object> approval, IEnumerable<Dictionary<string, object>> wftransactions, Dictionary<string, object> rejectedTransaction) {
            //locate assignment
            var assignment = wfassignment.FirstOrDefault(f => f[c.RoleIdColumn].ToString().EqualsIc(approvalGroup));
            approval["#shouldshowaction"] = user.HasPersonGroup(approvalGroup);
            if (assignment == null) {
                return;
            }
            var wfid = assignment[c.WfIdColumn];
            var txs = wftransactions.Where(tx => tx[c.WfIdColumn].ToString().Equals(wfid));
            var txToUse = txs.LastOrDefault();
            //locate the oldest tx of that assignment and use it´s data
            if (txToUse != null) {
                approval[c.ChangeByColumn] = txToUse[c.PersonIdColumn];
                approval[c.ChangeDateColumn] = txToUse[c.TransDateColumn];
                approval[c.StatusColumn] = GetStatusForPart1(rejectedTransaction, txToUse);
            }
            Log.DebugFormat("non customer approval handled " + string.Join(", ", approval.Select(m => m.Key + ":" + m.Value).ToArray()));
        }

        private void HandleCustomerApprovers(DataMap result, string approvalGroup, IEnumerable<Dictionary<string, object>> worklogs, IDictionary<string, object> approval) {
            var apprDescription = c.GetWorkLogDescriptions(approvalGroup, true);
            var rejDescription = c.GetWorkLogDescriptions(approvalGroup, false);

            var apprWl = worklogs.FirstOrDefault(w =>
                w["description"].ToString().EqualsIc(apprDescription)
                && w["logtype"].ToString().Equals(c.WlApprLogType));

            var rejWl = worklogs.FirstOrDefault(w =>
              w["description"].ToString().EqualsIc(rejDescription)
              && w["logtype"].ToString().Equals(c.WlRejLogType));

            var anyrejWl = worklogs.FirstOrDefault(w =>
              w["description"].ToString().StartsWith(c.RejectedWorklogDescription, StringComparison.CurrentCultureIgnoreCase)
              && w["logtype"].ToString().Equals(c.WlRejLogType));

            approval["#shouldshowaction"] = LevelMatches(result, approval);

            if (apprWl != null || rejWl != null) {
                approval[c.ChangeByColumn] = apprWl[c.CreateByColumn];
                approval[c.ChangeDateColumn] = apprWl[c.CreateDate];
                approval[c.StatusColumn] = apprWl != null ? c.ApprovedStatus : c.RejectedStatus;
                approval["#shouldshowaction"] = false;
            } else if (anyrejWl != null) {
                //if there´s a rejected worklog on the level, then all groups should be rejected, except the ones that might have approved it already...
                approval[c.StatusColumn] =  c.RejectedStatus;
            }


            Log.DebugFormat("customer approval handled " + string.Join(", ", approval.Select(m => m.Key + ":" + m.Value).ToArray()));
        }

        /// <summary>
        /// If the workflow is reject then the status would be reject if on the same transacationdate. This is because maximo would create 2 wftx at the same time.<p>
        /// 
        /// Otherwise, it would always be Approved
        /// </summary>
        /// <param name="rejectedTransaction"></param>
        /// <param name="txToUse"></param>
        /// <returns></returns>
        private string GetStatusForPart1(Dictionary<string, object> rejectedTransaction, Dictionary<string, object> txToUse) {
            if (rejectedTransaction == null) {
                return c.ApprovedStatus;
            }
            var sameAsRejected = rejectedTransaction[ApproverConstants.TransDateColumn].ToString().Equals(txToUse[ApproverConstants.TransDateColumn].ToString());
            return sameAsRejected ? ApproverConstants.RejectedStatus : ApproverConstants.ApprovedStatus;

        }

        private bool LevelMatches(DataMap change, IDictionary<string, object> approval) {
            var approverLevel = approval[ApproverConstants.ApprovalLevel.ToLower()];
            var changeLevel = change.GetAttribute(ApproverConstants.ChangeLevel.ToLower());
            Log.DebugFormat("approverLevel {0} changeLevel {1}", approverLevel, changeLevel);
            if (approverLevel == null) {
                return changeLevel == null;
            }
            if (changeLevel == null) {
                return false;
            }
            return approverLevel.ToString().EqualsIc(changeLevel.ToString());
        }


        public override string ApplicationName() {
            return "change";
        }
    }
}
