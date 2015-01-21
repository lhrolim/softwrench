using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Manchester {
    class ComWorkOrderCrudConnector : BaseWorkOrderCrudConnector {
        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeCreation(maximoTemplateData);
            var entity = (CrudOperationData)maximoTemplateData.OperationData;
            var maximoWo = maximoTemplateData.IntegrationObject;
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var entity = (CrudOperationData)maximoTemplateData.OperationData;
            var maximoWo = maximoTemplateData.IntegrationObject;
            //HandleTools(maximoTemplateData, entity, maximoWo);
            HandleMaterials(maximoTemplateData, entity, maximoWo);
        }

        private static void HandleTools(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData entity, object wo) {
            var tooltrans = (IEnumerable<CrudOperationData>)entity.GetRelationship("tooltrans");
            var newTools = tooltrans.Where(r => r.GetAttribute("tooltransid") == null);
            var recordKey = entity.Id;
            var user = SecurityFacade.CurrentUser();
            var crudOperationDatas = newTools as CrudOperationData[] ?? newTools.ToArray();

            WsUtil.CloneArray(crudOperationDatas, wo, "TOOLTRANS", delegate(object integrationObject, CrudOperationData crudData) {
                var itemsetid = (string)crudOperationDatas[0].GetAttribute("toolitem_.itemsetid");
                var qtyRequested = (Int64)WsUtil.GetRealValue(integrationObject, "TOOLQTY");
                var hrsRequested = (double)WsUtil.GetRealValue(integrationObject, "TOOLHRS");
                var rate = qtyRequested / hrsRequested;

                WsUtil.SetValue(integrationObject, "TOOLTRANSID", -1);

                WsUtil.SetValue(integrationObject, "TOOLQTY", qtyRequested);
                WsUtil.SetValue(integrationObject, "TOOLHRS", hrsRequested);
                WsUtil.SetValue(integrationObject, "TOOLRATE", rate);
                WsUtil.SetValue(integrationObject, "LINECOST", 0);
                WsUtil.SetValue(integrationObject, "OUTSIDE", false);
                WsUtil.SetValue(integrationObject, "ROLLUP", false);
                WsUtil.SetValue(integrationObject, "ENTEREDASTASK", false);
                WsUtil.SetValue(integrationObject, "ITEMSETID", itemsetid);
                WsUtil.SetValue(integrationObject, "ENTERBY", user.Login.ToUpper());
                WsUtil.SetValue(integrationObject, "ORGID", user.OrgId.ToUpper());
                WsUtil.SetValue(integrationObject, "SITEID", user.SiteId.ToUpper());
                WsUtil.SetValue(integrationObject, "REFWO", recordKey);
                WsUtil.SetValue(integrationObject, "ACTUALDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValue(integrationObject, "ENTERDATE", DateTime.Now.FromServerToRightKind(), true);


                ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Change.ToString());
            });
        }

        protected override void HandleMaterials(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData entity, object wo) {
            var materials = (IEnumerable<CrudOperationData>)entity.GetRelationship("matusetrans");
            var newMaterials = materials.Where(r => r.GetAttribute("matusetransid") == null);
            var recordKey = entity.Id;
            var user = SecurityFacade.CurrentUser();
            var crudOperationDatas = newMaterials as CrudOperationData[] ?? newMaterials.ToArray();

            WsUtil.CloneArray(crudOperationDatas, wo, "MATUSETRANS", delegate(object integrationObject, CrudOperationData crudData) {

                var cost = (double)WsUtil.GetRealValue(integrationObject, "UNITCOST");
                var quantity = (double)WsUtil.GetRealValue(integrationObject, "QTYREQUESTED");

                WsUtil.SetValueIfNull(integrationObject, "deleteForInsert", "false");
                // These values will be assigned 0 if linetype is MATERIAL
                WsUtil.SetValueIfNull(integrationObject, "CURBAL", 0);
                WsUtil.SetValueIfNull(integrationObject, "PHYSNT", 0);

                WsUtil.SetValue(integrationObject, "ACTUALCOST", cost);
                WsUtil.SetValue(integrationObject, "CONVERSION", 1);
                WsUtil.SetValue(integrationObject, "ROLLUP", 0);
                WsUtil.SetValue(integrationObject, "ISSUETYPE", "ISSUE");
                WsUtil.SetValue(integrationObject, "QUANTITY", (-1 * quantity));
                WsUtil.SetValue(integrationObject, "LINECOST", (cost * quantity));
                WsUtil.SetValue(integrationObject, "CURRENCYCODE", "USD");
                WsUtil.SetValue(integrationObject, "CURRENCYUNITCOST", cost);
                WsUtil.SetValue(integrationObject, "CURRENCYLINECOST", (cost * quantity));
                WsUtil.SetValue(integrationObject, "EXCHANGERATE", 1.00);
                WsUtil.SetValue(integrationObject, "ENTEREDASTASK", 0);
                WsUtil.SetValue(integrationObject, "CONDRATE", 100);
                WsUtil.SetValue(integrationObject, "TOSITE", user.SiteId);
                WsUtil.SetValue(integrationObject, "CONSIGMENT", 0);
                WsUtil.SetValue(integrationObject, "SPAREPARTADDED", 0);
                WsUtil.SetValue(integrationObject, "MATUSETRANSID", -1);
                WsUtil.SetValue(integrationObject, "ENTERBY", user.Login);
                WsUtil.SetValue(integrationObject, "ORGID", user.OrgId);
                WsUtil.SetValue(integrationObject, "SITEID", user.SiteId);
                WsUtil.SetValue(integrationObject, "REFWO", recordKey);
                WsUtil.SetValue(integrationObject, "ACTUALDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.FromServerToRightKind(), true);

                ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
            });
        }

        //protected override void HandleMaterials(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData entity, object wo) {
        //    var materials = (IEnumerable<CrudOperationData>)entity.GetRelationship("matusetrans");
        //    var newMaterials = materials.Where(r => r.GetAttribute("matusetransid") == null);
        //    var recordKey = entity.Id;
        //    var user = SecurityFacade.CurrentUser();
        //    var crudOperationDatas = newMaterials as CrudOperationData[] ?? newMaterials.ToArray();

        //    WsUtil.CloneArray(crudOperationDatas, wo, "MATUSETRANS", delegate(object integrationObject, CrudOperationData crudData) {
        //        var itemsetid = (string)crudOperationDatas[0].GetAttribute("item_.itemsetid");
        //        var qtyRequested = (double)WsUtil.GetRealValue(integrationObject, "QTYREQUESTED");
        //        WsUtil.SetValue(integrationObject, "MATUSETRANSID", -1);


        //        WsUtil.SetValue(integrationObject, "ITEMNUM", "PK-NO1-7006");
        //        WsUtil.SetValue(integrationObject, "STORELOC", "NO1");
        //        WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.FromServerToRightKind(), true);
        //        WsUtil.SetValue(integrationObject, "ACTUALDATE", DateTime.Now.FromServerToRightKind(), true);
        //        WsUtil.SetValue(integrationObject, "QUANTITY", -1);
        //        WsUtil.SetValue(integrationObject, "CURBAL", 3);
        //        WsUtil.SetValue(integrationObject, "PHYSCNT", 7);
        //        WsUtil.SetValue(integrationObject, "UNITCOST", 17.36);
        //        WsUtil.SetValue(integrationObject, "ACTUALCOST", 17.36);
        //        WsUtil.SetValue(integrationObject, "CONVERSION", 1);
        //        WsUtil.SetValue(integrationObject, "ASSETNUM", "400-153");
        //        WsUtil.SetValue(integrationObject, "ENTERBY", "JMALONG");
        //        WsUtil.SetValue(integrationObject, "MEMO", "123");
        //        WsUtil.SetValue(integrationObject, "OUTSIDE", false);
        //        WsUtil.SetValue(integrationObject, "ISSUETO", "DHELMS");
        //        WsUtil.SetValue(integrationObject, "ROLLUP", false);
        //        WsUtil.SetValue(integrationObject, "BINNUM", "AL001");
        //        WsUtil.SetValue(integrationObject, "ISSUETYPE", "ISSUE");
        //        WsUtil.SetValue(integrationObject, "GLDEBITACCT", "5030-100-101");
        //        WsUtil.SetValue(integrationObject, "LINECOST", 17.36);
        //        WsUtil.SetValue(integrationObject, "FINANCIALPERIOD", "QTR3_7");
        //        WsUtil.SetValue(integrationObject, "CURRENCYCODE", "USD");
        //        WsUtil.SetValue(integrationObject, "CURRENCYUNITCOST", 17.36);
        //        WsUtil.SetValue(integrationObject, "CURRENCYLINECOST", 17.36);
        //        WsUtil.SetValue(integrationObject, "LOCATION", "22335");
        //        WsUtil.SetValue(integrationObject, "DESCRIPTION", "FILTER  FUEL / CAT 1R-0751");
        //        WsUtil.SetValue(integrationObject, "EXCHANGERATE", 1);
        //        WsUtil.SetValue(integrationObject, "SPAREPARTADDED", false);
        //        WsUtil.SetValue(integrationObject, "QTYREQUESTED", 1);
        //        WsUtil.SetValue(integrationObject, "ORGID", "BSAKSS");
        //        WsUtil.SetValue(integrationObject, "SITEID", "KOD016");
        //        WsUtil.SetValue(integrationObject, "REFWO", "41772");
        //        WsUtil.SetValue(integrationObject, "ENTEREDASTASK", false);
        //        WsUtil.SetValue(integrationObject, "LINETYPE", "ITEM");
        //        WsUtil.SetValue(integrationObject, "ITEMSETID", "ITEM2");
        //        WsUtil.SetValue(integrationObject, "CONDRATE", 100);
        //        WsUtil.SetValue(integrationObject, "COMMODITYGROUP", "AUTO");
        //        WsUtil.SetValue(integrationObject, "COMMODITY", "FILTERS");
        //        WsUtil.SetValue(integrationObject, "TOSITEID", "KOD016");
        //        WsUtil.SetValue(integrationObject, "CONSIGNMENT", false);

        //        ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Delete.ToString());
        //    });
        //}
    }
}
