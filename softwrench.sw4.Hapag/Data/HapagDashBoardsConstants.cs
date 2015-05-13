using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.Hapag.Data {
    public class HapagDashBoardsConstants {
        
        public const string ActionRequiredForOpenRequestsOffering = "actionrequiredrequestoffering";
        public const string ActionRequiredForOpenRequests = "actionrequiredrequest";
        public const string EUOpenRequests = "euopenrequests";

        public const string ActionRequiredForOpenIncidents = "itcActionRequiredIncident";
        public const string OpenImacs = "itcOpenImacs";

        public const string OpenApprovals = "changeOpenApprovals";
        public const string OpenChangeTasks = "changeOpenTasks";

        public static string GetDefaultI18NValue(string key, bool isTooltip = false) {
            var value = string.Empty;

            switch (key) {
                case ActionRequiredForOpenRequests:
                    value = isTooltip ? "Your latest 5 Requests requiring Action" : "Action Required for Open Requests";
                    break;
                case EUOpenRequests:
                    value = isTooltip ? "Your latest 5 opened Requests" : "Open Requests";
                    break;
                case ActionRequiredForOpenIncidents:
                    value = isTooltip ? "Your latest 5 action required for open incidents" : "Action Required for Open Incidents";
                    break;
                case OpenImacs:
                    value = isTooltip ? "Your open IMAC Tasks" : "Open IMAC Tasks";
                    break;
                case OpenApprovals:
                    value = isTooltip ? "Your open Approvals" : "Open Approvals";
                    break;
                case OpenChangeTasks:
                    value = isTooltip ? "Your open Change Tasks" : "Open Change Tasks";
                    break;
            }
            return value;
        }
    }
}
