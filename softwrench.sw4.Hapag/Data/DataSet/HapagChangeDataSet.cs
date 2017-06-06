using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sw4.Hapag.Data.Init;
using softWrench.sW4.Data;
using softwrench.sw4.Hapag.Security;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Security.Context;
using c = softwrench.sw4.Hapag.Data.DataSet.Helper.ApproverConstants;

namespace softwrench.sw4.Hapag.Data.DataSet {
    public class HapagChangeDataSet : HapagBaseApplicationDataSet {

        private readonly ChangeGridUnionQueryGenerator _changeGridUnionQueryGenerator;
        private readonly ChangeDetailUnionQueryGenerator _changeDetailGenerator;
        private IContextLookuper _context;

        public HapagChangeDataSet(IHlagLocationManager locationManager, EntityRepository entityRepository, MaximoHibernateDAO maxDao,
            ChangeGridUnionQueryGenerator changeGridUnionQueryGenerator, ChangeDetailUnionQueryGenerator changeDetailGenerator, IContextLookuper context)
            : base(locationManager, entityRepository, maxDao) {
            _changeGridUnionQueryGenerator = changeGridUnionQueryGenerator;
            _changeDetailGenerator = changeDetailGenerator;
            _context = context;
        }

        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var ctx = _context.LookupContext();
            if (ctx.ApplicationLookupContext == null) {
                ctx.ApplicationLookupContext = new ApplicationLookupContext();
            }
            ctx.ApplicationLookupContext.MetadataId = "changedetail";

            return base.GetApplicationDetail(application, user, request);
        }


        public override CompositionFetchResult GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request,
            JObject currentData) {
            var compositionData = base.GetCompositionData(application, request, currentData);
            var user = SecurityFacade.CurrentUser();
            PopulateApprovalsCompositions(compositionData, user);
            return compositionData;
        }

        private void PopulateApprovalsCompositions(CompositionFetchResult compositionData, InMemoryUser user) {

            var compositions = compositionData.ResultObject;
            if (!compositions.ContainsKey("approvals_")) {
                return;
            }
            var approvals = compositions["approvals_"].ResultList as IEnumerable<IDictionary<string, object>>;
            var worklogs = (IEnumerable<Dictionary<string, object>>)compositions["worklog_"].ResultList;
            var wftransactions = (IEnumerable<Dictionary<string, object>>)compositions["apprwftransaction_"].ResultList;
            var wfassignment = (IEnumerable<Dictionary<string, object>>)compositions["wfassignment_"].ResultList;
            var wostatus = (IEnumerable<Dictionary<string, object>>)compositions["wostatus_"].ResultList;
            //let´s locate the first one which is marked as reject. 
            //it should always be the latest txlog, since no further operation is allowed after rejecting the workflow
            var rejectedTransaction = wftransactions.FirstOrDefault(w => w[ApproverConstants.TransTypeColumn].ToString().Equals("Reject"));

            int numberOfActions = 0;
            foreach (var approval in approvals) {
                var approvalGroup = (String)approval[c.ApproverGroupColumn];
                //non customer approvals
                if (approvalGroup != null && !approvalGroup.StartsWith("C-")) {
                    HandleNonCustomerApprovers(user, wfassignment, approvalGroup, approval, wftransactions, rejectedTransaction);
                } else if (approvalGroup != null) {
                    var needsApproval = HandleCustomerApprovers(user, compositionData.OriginalCruddata, approvalGroup, worklogs, approval, wostatus);
                    if (needsApproval) {
                        numberOfActions++;
                    }
                }
            }
            Log.DebugFormat("Number of Actions {0}", numberOfActions);
            compositionData.ParentModifiedFields["#numberofapprovalactions"] = numberOfActions;
        }

        private Boolean HandleNonCustomerApprovers(InMemoryUser user, IEnumerable<Dictionary<string, object>> wfassignment, string approvalGroup,
            IDictionary<string, object> approval, IEnumerable<Dictionary<string, object>> wftransactions, Dictionary<string, object> rejectedTransaction) {
            //locate assignment
            var assignment = wfassignment.FirstOrDefault(f => f[c.RoleIdColumn].ToString().EqualsIc(approvalGroup));
            approval["#shouldshowaction"] = user.HasPersonGroup(approvalGroup);
            if (assignment == null) {
                //nothing has been done, so it needs approval still
                return true;
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
            //TODO: is this logic correct? if the status of the approvergroup is already rejected why should we give the user any chance to change it?
            return !approval.ContainsKey(c.StatusColumn) || !approval[c.StatusColumn].EqualsAny(c.ApprovedStatus, c.RejectedStatus);
        }

        private bool HandleCustomerApprovers(InMemoryUser user, AttributeHolder result, string approvalGroup, IEnumerable<Dictionary<string, object>> worklogs, IDictionary<string, object> approval,
            IEnumerable<Dictionary<string, object>> wostatus) {



            //https://controltechnologysolutions.atlassian.net/browse/HAP-976
            //https://controltechnologysolutions.atlassian.net/browse/HAP-1113 --> changing the way it filters the status
            worklogs = FilterWorkLogsOlderThanLatestAuth(worklogs, wostatus);

            var wlEnumerable = worklogs as Dictionary<string, object>[] ?? worklogs.ToArray();

            var apprDescription = c.GetWorkLogDescriptions(approvalGroup, true);
            var rejDescription = c.GetWorkLogDescriptions(approvalGroup, false);


            var apprWl = wlEnumerable.FirstOrDefault(w =>
                apprDescription.EqualsIc(w["description"] as string)
                && c.WlApprLogType.EqualsIc(w["logtype"] as string)
                && w["recordkey"].Equals(result.GetAttribute("wonum"))
                );

            var rejWl = wlEnumerable.FirstOrDefault(w =>
                rejDescription.EqualsIc(w["description"] as string)
                && c.WlRejLogType.EqualsIc(w["logtype"] as string)
                && w["recordkey"].Equals(result.GetAttribute("wonum"))
                );

            var anyrejWl = wlEnumerable.FirstOrDefault(w =>
                 (w["description"] != null && w["description"].ToString().StartsWith(c.RejectedWorklogDescription, StringComparison.CurrentCultureIgnoreCase))
              && w["logtype"].ToString().Equals(c.WlRejLogType)
              && w["recordkey"].Equals(result.GetAttribute("wonum"))
              );

            //approval["#shouldshowaction"] = LevelMatches(result, approval) && user.HasPersonGroup(approvalGroup); ;
            //removed due to thomas comments, on HAP-976
            approval["#shouldshowaction"] = user.HasPersonGroup(approvalGroup);
            ;

            if (apprWl != null || rejWl != null) {

                Log.DebugFormat("Specific approval or rejected worklog found");

                approval[c.ChangeByColumn] = apprWl != null ? apprWl[c.CreateByColumn] : rejWl[c.CreateByColumn];
                approval[c.ChangeDateColumn] = apprWl != null ? apprWl[c.CreateDate] : rejWl[c.CreateDate];
                approval[c.StatusColumn] = apprWl != null ? c.ApprovedStatus : c.RejectedStatus;
                approval["#shouldshowaction"] = false;
            } else if (anyrejWl != null) {

                Log.DebugFormat("At least one rejected worklog found scenario");
                //if there´s a rejected worklog on the level, then all groups should be rejected, except the ones that might have approved it already...
                approval[c.StatusColumn] = c.RejectedStatus;
                //HAP-993 if any of the groups rejected it, we should no longer display the actions
                approval["#shouldshowaction"] = false;
            }
            Log.DebugFormat("customer approval handled " + string.Join(", ", approval.Select(m => m.Key + ":" + m.Value).ToArray()));
            //if this group has not yet been approved or rejectd, it will still require some action (maybe not by this user)
            return !approval.ContainsKey(c.StatusColumn) || !approval[c.StatusColumn].EqualsAny(c.ApprovedStatus, c.RejectedStatus);
        }

        private IEnumerable<Dictionary<string, object>> FilterWorkLogsOlderThanLatestAuth(IEnumerable<Dictionary<string, object>> worklogs, IEnumerable<Dictionary<string, object>> wostatuse) {
            DateTime? latestAuthStatusDate = null;
            foreach (var status in wostatuse) {
                if (status["status"].EqualsIc("AUTH")) {
                    latestAuthStatusDate = status["changedate"] as DateTime?;
                    break;
                }
            }
            if (latestAuthStatusDate == null) {
                return worklogs;
            }

            Log.DebugFormat("Auth status date found {0}".Fmt(latestAuthStatusDate));

            //remove any worklogs older than the last date that the change has been marked as AUTH
            var resultList = new List<Dictionary<string, object>>();
            foreach (var worklog in worklogs) {
                var wlDate = worklog["itdcreatedate"] as DateTime?;
                if (wlDate > latestAuthStatusDate) {
                    Log.DebugFormat("Adding worklog {0}, with date {1} to list", worklog["worklogid"], worklog["itdcreatedate"]);
                    resultList.Add(worklog);
                }
            }
            return resultList;
        }

        private DateTime? FetchLatestWOStatusDate(IEnumerable<Dictionary<string, object>> wostatuses) {
            foreach (var wostatus in wostatuses.Where(wostatus => wostatus["status"].EqualsIc("AUTH"))) {
                return wostatus["changedate"] as DateTime?;
            }
            return null;
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

        public string GenerateUnionQuery(EntityMetadata entity, SearchRequestDto searchDTO) {
            var context = ContextLookuper.LookupContext();
            var isTomOrItom = context.IsInModule(FunctionalRole.Itom) || context.IsInModule(FunctionalRole.Tom);
            if (isTomOrItom) {
                //HAP-805, tom or itom roles can see everything in the grid
                return null;
            }

            return _changeGridUnionQueryGenerator.GenerateQuery(entity, searchDTO);
        }

        public string GenerateUnionQueryCount(EntityMetadata entity, SearchRequestDto searchDTO) {
            var context = ContextLookuper.LookupContext();
            var isTomOrItom = context.IsInModule(FunctionalRole.Itom) || context.IsInModule(FunctionalRole.Tom);
            if (isTomOrItom) {
                //HAP-805, tom or itom roles can see everything in the grid
                return null;
            }
            return _changeGridUnionQueryGenerator.GenerateQueryCount(entity, searchDTO);
        }



        public override string ApplicationName() {
            return "change";
        }
    }
}
