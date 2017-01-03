using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using WebGrease.Css.Extensions;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Compositions {
    public class LabTransHandler : ISingletonComponent {

        private const string LaborAttribute = "laborcode";
        private const string CraftAttribute = "craft";
        private const string PayRateAttribute = "payrate";

        private readonly MaximoHibernateDAO _maxDAO;

        public LabTransHandler(MaximoHibernateDAO maxDAO) {
            _maxDAO = maxDAO;
        }


        [Transactional(DBType.Maximo)]
        public virtual void HandleLabors(CrudOperationData entity, object wo) {
            

            // Filter work order materials for any new entries where matusetransid is null
            var labors = (IEnumerable<CrudOperationData>)entity.GetRelationship("labtrans");
            var newLabors = labors.Where(r => r.GetAttribute("labtransid") == null);
            var deletedLabors = labors.Where(r => r.GetAttribute("labtransid") != null && r.ContainsAttribute("#deleted"));
            var editedLabors = labors.Where(r => r.GetAttribute("labtransid") != null && r.ContainsAttribute("#edited"));
            if (editedLabors.Any()) {
                editedLabors.ForEach(e => e.UnmappedAttributes.Remove("#laborlist_"));
            }
            var modifiedLabors = newLabors.Concat(deletedLabors).Concat(editedLabors);

            // Convert collection into array, if any are available
            //var crudOperationData = newLabors as CrudOperationData[] ?? newLabors.ToArray();
            var crudOperationData = modifiedLabors as CrudOperationData[] ?? modifiedLabors.ToArray();

            var parsedOperationData = new List<CrudOperationData>();
            HandlerParseUtil.ParseUnmappedCompositionInline(crudOperationData, parsedOperationData, "#laborlist_",
                delegate (CrudOperationData newcrudOperationData, JObject jsonObject) {
                    newcrudOperationData.Fields[LaborAttribute] = jsonObject.Value<string>(LaborAttribute);
                    newcrudOperationData.Fields[CraftAttribute] = jsonObject.Value<string>(CraftAttribute);
                    newcrudOperationData.Fields[PayRateAttribute] = jsonObject.Value<double>(PayRateAttribute);
                });

            var deletion = false;

            WsUtil.CloneArray(parsedOperationData, wo, "LABTRANS",
                delegate (object integrationObject, CrudOperationData crudData) {
                    // If deleted, 
                    if (crudData.ContainsAttribute("#deleted") && crudData.GetAttribute("#deleted").ToString() == "1") {
                        //TODO:Workaround while we´re figuring out how to do it via WebServices
                        deletion = true;
                        DeleteLabtrans(crudData.GetAttribute("labtransid").ToString());

                        //                        var payRate = GetPayRate(crudData);
                        //                        WsUtil.SetValueIfNull(integrationObject, "PAYRATE", payRate);
                        //                        WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.AddMinutes(-1).FromServerToRightKind(),
                        //                            true);
                        //                        WsUtil.SetValue(integrationObject, "ENTERDATE", DateTime.Now.AddMinutes(-1).FromServerToRightKind(),
                        //                            true);
                        //                        ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Delete.ToString());



                    } else if (crudData.ContainsAttribute("#edited") && crudData.GetAttribute("#edited").ToString() == "1") {
                        DeleteLabtrans(crudData.GetAttribute("labtransid").ToString());
                        AddLabtrans(entity, integrationObject, crudData);
                    } else {
                        AddLabtrans(entity, integrationObject, crudData);
                    }
                });
            if (deletion) {
                ReflectionUtil.SetProperty(wo, "LABTRANS", null);
                //                WsUtil.SetValue(wo, "LABTRANS", null);
            }

        }

        private static void FillSiteId(CrudOperationData crudData, InMemoryUser user, object integrationObject) {
            //            var laborRel = ((Entity)crudData.GetRelationship("labor_"));
            //
            //            if (laborRel == null) {
            //                //this is only null in the scenario where the labor was selected based upon the default user selection, and no change was made.
            //                //in that case, let´s just use the user´s default values.
            //                //SWWEB-1965 item 8
            //                WsUtil.SetValue(integrationObject, "ORGID", user.OrgId);
            //                WsUtil.SetValue(integrationObject, "SITEID", user.SiteId);
            //                return;
            //            }
            //            //logic is we need to use the same siteId/Orgid from the labor, falling back to the currentUser
            //            var woSite = laborRel.GetAttribute("worksite");
            //            var orgId = laborRel.GetAttribute("orgid");
            //            if (woSite == null) {
            //                woSite = user.SiteId;
            //            }
            //            if (orgId == null) {
            //                orgId = user.OrgId;
            //            }


            WsUtil.SetValue(integrationObject, "ORGID", crudData.GetAttribute("orgid"));
            WsUtil.SetValue(integrationObject, "SITEID", crudData.GetAttribute("siteid"));
        }

        private static object GetPayRate(CrudOperationData crudData) {
            var entity = ((Entity)crudData.GetRelationship("laborcraftrate_"));
            object rate = null;
            if (entity != null) {
                rate = entity.GetAttribute("rate");
            } else {
                rate = SecurityFacade.CurrentUser().GetProperty("defaultcraftrate");
            }
            if (rate == null) {
                return 0.0;
            }
            var d = (decimal)rate;
            return Convert.ToDouble(d);
        }

        private static void FillLineCostLabor(object integrationObject) {
            try {
                var payRateAux = WsUtil.GetRealValue(integrationObject, "PAYRATE");
                double payRate;
                double.TryParse(payRateAux.ToString(), out payRate);
                var regularHrsAux = WsUtil.GetRealValue(integrationObject, "REGULARHRS");
                double regularHrs;
                double.TryParse(regularHrsAux.ToString(), out regularHrs);
                var lineCost = (payRate * regularHrs);
                WsUtil.SetValue(integrationObject, "LINECOST", lineCost);
            } catch {
                WsUtil.SetValue(integrationObject, "LINECOST", null);
            }
        }

        [Transactional(DBType.Maximo)]
        public virtual void DeleteLabtrans(string labtransid) {
            _maxDAO.ExecuteSql("delete from labtrans where labtransid = ? ", labtransid);
        }

        private void AddLabtrans(CrudOperationData entity, object integrationObject, CrudOperationData crudData) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();
            // Workorder id used for data association
            var recordKey = entity.UserId;

            var transType = "WORK";
            if (crudData.ContainsAttribute("transtype")) {
                transType = crudData.GetStringAttribute("transtype");
            }

            WsUtil.SetValue(integrationObject, "LABTRANSID", -1);
            WsUtil.SetValue(integrationObject, "REFWO", recordKey);
            WsUtil.SetValue(integrationObject, "TRANSTYPE", transType);


            WsUtil.SetValue(integrationObject, "ORGID", entity.GetAttribute("orgid"));
            WsUtil.SetValue(integrationObject, "SITEID", entity.GetAttribute("siteid"));


            WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.AddMinutes(-1).FromServerToRightKind(),
                true);
            WsUtil.SetValue(integrationObject, "ENTERDATE", DateTime.Now.AddMinutes(-1).FromServerToRightKind(),
                true);
            WsUtil.SetValueIfNull(integrationObject, "LABORCODE", user.Login.ToUpper());
            WsUtil.SetValueIfNull(integrationObject, "ENTERBY", user.Login.ToUpper());
            var payRate = GetPayRate(crudData);


            WsUtil.SetValueIfNull(integrationObject, "PAYRATE", payRate);
            // Maximo 7.6 Changes
            DateTime startdateentered;
            var jsonDate = crudData.GetAttribute("startdate");
            var parsedDate = jsonDate as DateTime?;
            if (parsedDate != null) {
                //if already a date, it was parsed on ConversionUTIL
                WsUtil.SetValueIfNull(integrationObject, "STARTDATEENTERED", parsedDate, true);
            } else if (jsonDate != null && DateTime.TryParse(jsonDate.ToString(), out startdateentered)) {
                WsUtil.SetValueIfNull(integrationObject, "STARTDATEENTERED",
                    DateUtil.BeginOfDay(startdateentered).FromServerToRightKind(), true);
            }
            ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
            FillLineCostLabor(integrationObject);
        }
    }
}
