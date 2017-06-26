using System;
using cts.commons.portable.Util;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softwrench.sw4.pae.classes.com.cts.pae.connector {
    public class PaeAssetCrudConnector : BaseAssetCrudConnector {

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var asset = maximoTemplateData.IntegrationObject;
            var currentTime = DateTime.Now.FromServerToRightKind();
            var crudOperationData = (CrudOperationData)maximoTemplateData.OperationData;

            var dateToUse = currentTime;
            var personId = CurrentPersonId();

            var isTransportationAsset = "transportation".EqualsIc(crudOperationData.ApplicationMetadata.Name);
            var isOfflineSave = crudOperationData.ContainsAttribute("#offlinesavedate");

            if (isOfflineSave) {
                // this means it's coming from offline
                var offlineDate = crudOperationData.GetUnMappedAttribute("#offlinesavedate");
                dateToUse = HandleOfflineDate(offlineDate);
            }

            w.SetValue(asset, "invdate", dateToUse);
            w.SetValue(asset, "invpostdate", dateToUse);
            w.SetValue(asset, "invpostdateby", personId);
            w.SetValue(asset, "invposttype", "Automatic");

            // Additional required fields
            w.SetValueIfNull(asset, "plustsoldamt", 0);
            w.SetValueIfNull(asset, "plusttotalmprevenue", 0);
            w.SetValueIfNull(asset, "taxpercent", 0);

            if (isTransportationAsset && !isOfflineSave) {
                w.SetValue(asset, "CHANGEDATE", dateToUse);
                w.SetValue(asset, "CHANGEBY", personId);
            }

            HandleAssetTrans(asset, crudOperationData, dateToUse);

            base.BeforeUpdate(maximoTemplateData);
        }

        public override string ApplicationName() {
            return "asset,paeasset";
        }


        public override string ClientFilter() {
            return "pae";
        }


        private static DateTime HandleOfflineDate(string offlineDate) {
            var date = Convert.ToDateTime(offlineDate);
            DateTime.SpecifyKind(date, DateTimeKind.Utc);
            return date;
        }

        private static void HandleAssetTrans(object asset, CrudOperationData operationData, DateTime dateToUse) {
            var location = operationData.GetAttribute("location");
            var originalLocation = operationData.GetAttribute("#originallocation");
            if ((originalLocation == null && location == null) || (location != null && location.Equals(originalLocation))) {
                return;
            }
            var assetTransArr = ReflectionUtil.InstantiateArrayWithBlankElements(asset, "ASSETTRANS", 1);
            var assetTrans = assetTransArr.GetValue(0);
            w.SetValue(assetTrans, "TOLOC", location);
            w.SetValue(assetTrans, "FROMLOC", originalLocation);

            var personId = CurrentPersonId();

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

        private static string CurrentPersonId() {
            return SecurityFacade.CurrentUser().MaximoPersonId ?? "MAXADMIN";
        }
    }
}
