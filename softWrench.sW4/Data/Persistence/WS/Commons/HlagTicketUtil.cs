using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    public class HlagTicketUtil {

        public static bool IsIBMTicket(CrudOperationData datamap) {
            return datamap.ContainsAttribute("ownergroup") &&
                datamap.GetAttribute("ownergroup") != null &&
                !datamap.GetAttribute("ownergroup").ToString().StartsWith("C-");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="isCreation">new ticket or update of existing one</param>
        /// <param name="jsonObject">data comming from screen</param>
        /// <param name="defaultGroup">either I-EUS-DE-CSC-SDK-HLCFRONTDESKI or I-EUS-DE-CSC-SDK-HLCFRONTDESK</param>
        /// <returns></returns>
        public static String HandleSRAndIncidentOwnerGroups(bool isCreation, CrudOperationData jsonObject, string defaultGroup) {
            if (isCreation) {
                return defaultGroup;
            }

            var status = (string)jsonObject.GetAttribute("status");
            var groupAlreadyAssociated = jsonObject.GetAttribute("ownergroup") as string;
            var isIbmTicket = IsIBMTicket(jsonObject);
            var nullOwner = string.IsNullOrEmpty((string)jsonObject.GetAttribute("owner"));

            if ("SLAHOLD".Equals(status, StringComparison.CurrentCultureIgnoreCase)) {

                if (!isIbmTicket && nullOwner) {
                    return defaultGroup;
                }
                return groupAlreadyAssociated;
            }

            if ("true".Equals(jsonObject.GetAttribute("#submittingaction")) && !isIbmTicket && jsonObject.GetAttribute("status").Equals("QUEUED")) {
                //this means that we´re handling the action disagree since (RESOLVED status workflow)
                //these variables would have already set this to "QUEUED" in the SubmitActionCrudConnector class
                if (nullOwner ) {
                    return defaultGroup;
                }
                //(if an Owner is associated it should not be overhanded again)
                return groupAlreadyAssociated;
            }

            //this would be called only for attachments/worklogs, the remaining case
            return defaultGroup;

        }
    }
}
