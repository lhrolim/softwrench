using System;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class PaeAssetCrudConnector : CrudConnectorDecorator {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var asset = maximoTemplateData.IntegrationObject;
            var currentTime = DateTime.Now.FromServerToRightKind();
            var crudOperationData = (CrudOperationData)maximoTemplateData.OperationData;

            var dateToUse = currentTime;
            var user = SecurityFacade.CurrentUser();
            if (crudOperationData.ContainsAttribute("#offlinesavedate")) {
                //this means it´s coming from offline
                dateToUse = HandleOfflineDate(crudOperationData);
            }
            w.SetValue(asset, "invdate", dateToUse);
            w.SetValue(asset, "invpostdate", dateToUse);

            w.SetValue(asset, "invpostdateby", user.DBUser.UserName);
            w.SetValue(asset, "invposttype", "Automatic");

            // Additional required fields
            w.SetValueIfNull(asset, "plustsoldamt", 0);
            w.SetValueIfNull(asset, "plusttotalmprevenue", 0);
            w.SetValueIfNull(asset, "taxpercent", 0);


            HandleAssetTrans(asset, crudOperationData, dateToUse);

            base.BeforeUpdate(maximoTemplateData);
        }

        private static DateTime HandleOfflineDate(CrudOperationData crudOperationData) {
            //this only occurs when it comes from the offline
            var scanDate = crudOperationData.GetUnMappedAttribute("#offlinesavedate");
            var datescanDate = Convert.ToDateTime(scanDate);
            DateTime.SpecifyKind(datescanDate, DateTimeKind.Utc);
            return datescanDate;
        }

        private void HandleAssetTrans(object asset, CrudOperationData operationData, DateTime dateToUse) {

            if ((operationData.GetAttribute("#originallocation") == null && operationData.GetAttribute("location") == null)
                || operationData.GetAttribute("location").Equals(operationData.GetAttribute("#originallocation"))) {
                return;
            }

            

            var assetTransArr = ReflectionUtil.InstantiateArrayWithBlankElements(asset, "ASSETTRANS", 1);
            var assetTrans = assetTransArr.GetValue(0);
            w.SetValue(assetTrans, "TOLOC", operationData.GetAttribute("location"));
            w.SetValue(assetTrans, "FROMLOC", operationData.GetAttribute("#originallocation"));

            var personId = SecurityFacade.CurrentUser().MaximoPersonId ?? "MAXADMIN";

            w.SetValue(assetTrans, "ENTERBY", personId);
            w.SetValue(assetTrans, "DATEMOVED", dateToUse);
            w.SetValue(assetTrans, "TRANSDATE", dateToUse);
            w.SetValue(assetTrans, "TRANSTYPE", "MOVED");
            w.SetValue(assetTrans, "ASSETID", operationData.GetAttribute("assetid"));
            w.SetValue(assetTrans, "ORGID", operationData.GetAttribute("orgid"));
            w.SetValue(assetTrans, "SITEID", operationData.GetAttribute("siteid"));
            w.SetValue(assetTrans, "TOORGID", operationData.GetAttribute("orgid"));
            w.SetValue(assetTrans, "TOSITEID", operationData.GetAttribute("siteid"));

        }
    }
}
