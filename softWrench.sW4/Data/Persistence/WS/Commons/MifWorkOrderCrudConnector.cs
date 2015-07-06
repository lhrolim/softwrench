using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Applications.Workorder;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class MifWorkOrderCrudConnector : BaseWorkOrderCrudConnector {

        protected override void HandleLabors(CrudOperationData entity, object maximoWo) {
            var labors = (IEnumerable<CrudOperationData>)entity.GetRelationship("labtrans");
            var newLabors = labors.Where(r => r.GetAttribute("labtransid") == null);

            var user = SecurityFacade.CurrentUser();
            WsUtil.CloneArray(newLabors, maximoWo, "LABTRANS",
                delegate(object integrationObject, CrudOperationData crudData) {
                    if (ReflectionUtil.IsNull(integrationObject, "LABTRANSID")) {
                        WsUtil.SetValue(integrationObject, "LABTRANSID", -1);
                        WsUtil.SetValue(integrationObject, "ORGID", user.OrgId);
                        WsUtil.SetValue(integrationObject, "SITEID", user.SiteId);
                        WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.FromServerToRightKind(), true);
                        WsUtil.SetValue(integrationObject, "ENTERDATE", DateTime.Now.FromServerToRightKind(), true);
                        FillLineCostLabor(integrationObject);
                    }
                });
        }

        private static void FillLineCostLabor(object integrationObject) {
            try {
                var payRateAux = WsUtil.GetRealValue(integrationObject, "PAYRATE");
                double payRate;
                double.TryParse(payRateAux.ToString(), out payRate);
                var regularHrsAux = WsUtil.GetRealValue(integrationObject, "REGULARHRS");
                int regularHrs;
                int.TryParse(regularHrsAux.ToString(), out regularHrs);
                var lineCost = (payRate * regularHrs);
                WsUtil.SetValue(integrationObject, "LINECOST", lineCost);
            }
            catch {
                WsUtil.SetValue(integrationObject, "LINECOST", null);
            }
        }

//        protected override void HandleMaterials(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData entity, object wo) {
//            var materials = (IEnumerable<CrudOperationData>)entity.GetRelationship("matusetrans");
//            var newMaterials = materials.Where(r => r.GetAttribute("matusetransid") == null);
//            var recordKey = entity.Id;
//            var user = SecurityFacade.CurrentUser();
//            WsUtil.CloneArray(newMaterials, wo, "MATUSETRANS", delegate(object integrationObject, CrudOperationData crudData) {
//                var realValue = (double)WsUtil.GetRealValue(integrationObject, "QTYREQUESTED");
//                WsUtil.SetValue(integrationObject, "QTYREQUESTED", -1 * realValue);
//                WsUtil.SetValue(integrationObject, "MATUSETRANSID", -1);
//                WsUtil.SetValue(integrationObject, "ENTERBY", user.Login);
//                WsUtil.SetValue(integrationObject, "ORGID", user.OrgId);
//                WsUtil.SetValue(integrationObject, "SITEID", user.SiteId);
//                WsUtil.SetValue(integrationObject, "REFWO", recordKey);
//                WsUtil.SetValue(integrationObject, "ACTUALDATE", DateTime.Now.ToUserTimezone(user), true);
//                WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.ToUserTimezone(user), true);
//
//                ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
//            });
//        }
    }
}
