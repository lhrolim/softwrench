using System;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class PaeAssetCrudConnector : CrudConnectorDecorator {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var asset = maximoTemplateData.IntegrationObject;
            var currentTime = DateTime.Now.FromServerToRightKind();
            var crudOperationData = (CrudOperationData)maximoTemplateData.OperationData;
            var dateToUse = crudOperationData.ContainsAttribute("#scandate")
                ? crudOperationData.GetAttribute("#scandate")
                : currentTime;

            w.SetValue(asset, "invdate", dateToUse);
            w.SetValue(asset, "invpostdate", dateToUse);

            w.SetValue(asset, "invpostdateby", SecurityFacade.CurrentUser().DBUser.UserName);
            w.SetValue(asset, "invposttype", "Automatic");

            // Additional required fields
            w.SetValueIfNull(asset, "plustsoldamt", 0);
            w.SetValueIfNull(asset, "plusttotalmprevenue", 0);
            w.SetValueIfNull(asset, "taxpercent", 0);


            HandleAssetTrans(asset, crudOperationData);

            base.BeforeUpdate(maximoTemplateData);
        }

        private void HandleAssetTrans(object asset, CrudOperationData operationData) {

            if ((operationData.GetAttribute("#originallocation") == null && operationData.GetAttribute("location") == null)
                || operationData.GetAttribute("#originallocation").Equals(operationData.GetAttribute("location"))) {
                return;
            }

            var currentTime = DateTime.Now;

            var assetTransArr = ReflectionUtil.InstantiateArrayWithBlankElements(asset, "ASSETTRANS", 1);
            var assetTrans = assetTransArr.GetValue(0);
            w.SetValue(assetTrans, "TOLOC", operationData.GetAttribute("location"));
            w.SetValue(assetTrans, "FROMLOC", operationData.GetAttribute("#originallocation"));

            var personId = SecurityFacade.CurrentUser().MaximoPersonId ?? "MAXADMIN";

            w.SetValue(assetTrans, "ENTERBY", personId);
            w.SetValue(assetTrans, "DATEMOVED", currentTime);
            w.SetValue(assetTrans, "TRANSDATE", currentTime);
            w.SetValue(assetTrans, "TRANSTYPE", "MOVED");
            w.SetValue(assetTrans, "ASSETID", operationData.GetAttribute("assetid"));
            w.SetValue(assetTrans, "ORGID", operationData.GetAttribute("orgid"));
            w.SetValue(assetTrans, "SITEID", operationData.GetAttribute("siteid"));
            w.SetValue(assetTrans, "TOORGID", operationData.GetAttribute("orgid"));
            w.SetValue(assetTrans, "TOSITEID", operationData.GetAttribute("siteid"));

        }
    }
}
