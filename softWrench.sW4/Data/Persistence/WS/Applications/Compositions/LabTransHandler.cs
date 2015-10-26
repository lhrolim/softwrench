﻿using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    class LabTransHandler {

        public static void HandleLabors(CrudOperationData entity, object wo) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Workorder id used for data association
            var recordKey = entity.UserId;

            // Filter work order materials for any new entries where matusetransid is null
            var labors = (IEnumerable<CrudOperationData>)entity.GetRelationship("labtrans");
            var newLabors = labors.Where(r => r.GetAttribute("labtransid") == null);

            // Convert collection into array, if any are available
            var crudOperationData = newLabors as CrudOperationData[] ?? newLabors.ToArray();

            if (crudOperationData.Length > 1) {
                crudOperationData = crudOperationData.Skip(crudOperationData.Length - 1).ToArray();
            }



            WsUtil.CloneArray(crudOperationData, wo, "LABTRANS", delegate (object integrationObject, CrudOperationData crudData) {

                WsUtil.SetValue(integrationObject, "LABTRANSID", -1);
                WsUtil.SetValue(integrationObject, "REFWO", recordKey);
                WsUtil.SetValue(integrationObject, "TRANSTYPE", "WORK");

                FillSiteId(crudData, user, integrationObject);

                WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValue(integrationObject, "ENTERDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValueIfNull(integrationObject, "LABORCODE", user.Login.ToUpper());
                WsUtil.SetValueIfNull(integrationObject, "ENTERBY", user.Login.ToUpper());
                var payRate = GetPayRate(crudData);


                WsUtil.SetValueIfNull(integrationObject, "PAYRATE", payRate);
                // Maximo 7.6 Changes
                DateTime startdateentered;
                if (crudData.GetAttribute("startdate") != null && DateTime.TryParse(crudData.GetAttribute("startdate").ToString(), out startdateentered)) {
                    WsUtil.SetValueIfNull(integrationObject, "STARTDATEENTERED", startdateentered.FromServerToRightKind(), true);
                }
                ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
                FillLineCostLabor(integrationObject);
            });
        }

        private static void FillSiteId(CrudOperationData crudData, InMemoryUser user, object integrationObject) {
            var laborRel = ((Entity)crudData.GetRelationship("labor_"));

            //logic is we need to use the same siteId/Orgid from the labor, falling back to the currentUser
            var woSite = laborRel.GetAttribute("worksite");
            var orgId = laborRel.GetAttribute("orgid");
            if (woSite == null) {
                woSite = user.SiteId;
            }
            if (orgId == null) {
                orgId = user.OrgId;
            }


            WsUtil.SetValue(integrationObject, "ORGID", orgId);
            WsUtil.SetValue(integrationObject, "SITEID", woSite);
        }

        private static object GetPayRate(CrudOperationData crudData) {
            var attribute = ((Entity)crudData.GetRelationship("laborcraftrate_")).GetAttribute("rate");
            if (attribute == null) {
                return 0.0;
            }
            var d = (decimal)attribute;
            return Convert.ToDouble(d);
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
            } catch {
                WsUtil.SetValue(integrationObject, "LINECOST", null);
            }
        }
    }
}