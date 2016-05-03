using Newtonsoft.Json;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    public class ServiceAddressHandler {

        public static void HandleServiceAddressForWo(CrudOperationData entity, object wo) {
            var hasServiceAddressChange = entity.GetAttribute("#haswoaddresschange") != null;
            if (!hasServiceAddressChange) {
                return;
            }

            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Create a new WOSERVICEADDRESS instance created
            var woserviceaddress = ReflectionUtil.InstantiateSingleElementFromArray(wo, "WOSERVICEADDRESS");

            // Extract data from unmapped attribute
            var json = entity.GetUnMappedAttribute("#woaddress_");

            // If empty, we assume there's no selected data.  
            if (json != null) {
                dynamic woaddress = JsonConvert.DeserializeObject(json);

                //do not change these to var
                string addresscode = woaddress.addresscode;
                string desc = woaddress.description;
                string straddrnumber = woaddress.staddrnumber;
                string straddrstreet = woaddress.staddrstreet;
                string straddrtype = woaddress.staddrtype;

                WsUtil.SetValue(woserviceaddress, "SADDRESSCODE", addresscode);
                WsUtil.SetValue(woserviceaddress, "DESCRIPTION", desc);
                WsUtil.SetValue(woserviceaddress, "STADDRNUMBER", straddrnumber);
                WsUtil.SetValue(woserviceaddress, "STADDRSTREET", straddrstreet);
                WsUtil.SetValue(woserviceaddress, "STADDRSTTYPE", straddrtype);
            } else {
                WsUtil.SetValueIfNull(woserviceaddress, "STADDRNUMBER", "");
                WsUtil.SetValueIfNull(woserviceaddress, "STADDRSTREET", "");
                WsUtil.SetValueIfNull(woserviceaddress, "STADDRSTTYPE", "");
            }

            var prevWOServiceAddress = entity.GetRelationship("woserviceaddress");

            if (prevWOServiceAddress != null) {
                WsUtil.SetValue(woserviceaddress, "FORMATTEDADDRESS", ((CrudOperationData)prevWOServiceAddress).GetAttribute("formattedaddress") ?? "");
            }

            //WsUtil.SetValueIfNull(woserviceaddress, "WOSERVICEADDRESSID", -1);          
            WsUtil.SetValue(woserviceaddress, "ORGID", user.OrgId);
            WsUtil.SetValue(woserviceaddress, "SITEID", user.SiteId);

            ReflectionUtil.SetProperty(woserviceaddress, "action", OperationType.AddChange.ToString());
        }

    }
}
