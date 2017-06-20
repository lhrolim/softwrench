﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using NHibernate.Util;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.dataset;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt.email;
using softwrench.sw4.Shared2.Data.Association;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.opt {
    public class FirstSolarMaintenanceEngineeringHandler : ISingletonComponent {

        [Import]
        public ISWDBHibernateDAO Dao { get; set; }

        [Import]
        public IMaximoHibernateDAO MaxDao { get; set; }

        [Import]
        public FirstSolarMaintenanceEmailService MaintenanceEmailService { get; set; }

        [Import]
        public FirstSolarWorkPackageAttachmentsHandler AttachmentsHandler { get; set; }

        private const string FilterPrefix = "swwpkgme:";
        private const string AttachmentsRelationship = "#maintenanceengineeringfileexplorer_";

        public void HandleAttachmentsOnCompositionLoad(CompositionFetchResult woResult, CompositionFetchResult packageResult) {
            AttachmentsHandler.HandleAttachmentsOnCompositionLoad(woResult, packageResult, AttachmentsRelationship, FSWPackageConstants.MaintenanceEngAttachsRelationship);
        }

        public void HandleMaintenanceEngs(CrudOperationData crudoperationData, WorkPackage package, CrudOperationData woData) {
            var existingMaintenanceEng = package.MaintenanceEngineerings;
            package.MaintenanceEngineerings = new List<MaintenanceEngineering>();
            if (crudoperationData.AssociationAttributes != null && crudoperationData.AssociationAttributes.ContainsKey("maintenanceEngineerings_")) {
                var maintenanceEngsData = crudoperationData.AssociationAttributes["maintenanceEngineerings_"] as List<CrudOperationData>;
                if (maintenanceEngsData == null) {
                    throw new Exception("Incorrect format of maintenance engineering list.");
                }
                maintenanceEngsData.ForEach((data) => {
                    package.MaintenanceEngineerings.Add(HandleMaintenanceEng(data, GetOurCreateMaintenanceEng(data, existingMaintenanceEng), package, woData));
                });
            }
            existingMaintenanceEng?.ForEach(me => {
                if (me.Status.IsSubmitted()) {
                    throw new Exception($"Is not possible delete a maintenance engineering request with status '{me.Status}'. Reload the page to get the updated version of this work package.");
                }
                Dao.Delete(me);
            });
        }

        public async Task HandleEmails(WorkPackage package, string siteId, IEnumerable<MaintenanceEngineering> mesToSend) {
            await AttachmentsHandler.HandleEmails(package, siteId, FSWPackageConstants.MaintenanceEngAttachsRelationship, FilterPrefix, mesToSend, MaintenanceEmailService);
        }

        public void HandleEmail(MaintenanceEngineering me, WorkPackage package, string siteId) {
            AttachmentsHandler.HandleEmail(me, package, siteId, FilterPrefix, FSWPackageConstants.MaintenanceEngAttachsRelationship, MaintenanceEmailService);
        }

        public void AddEngineerAssociations(ApplicationDetailResult result) {
            if (!result.ResultObject.ContainsKey("maintenanceEngineerings_")) {
                return;
            }
            var mes = result.ResultObject["maintenanceEngineerings_"] as List<Dictionary<string, object>>;
            var woSiteObj = (object)null;
            result.ResultObject.TryGetValue("#workorder_.siteid", out woSiteObj);
            result.AssociationOptions.PreFetchLazyOptions.Add("#workorder_.fakelabor_", SetEngineerNames(mes, woSiteObj as string));
        }

        public void LoadEngineerNames(CompositionFetchResult compList, string woSite) {
            if (!compList.ResultObject.ContainsKey("maintenanceEngineerings_")) {
                return;
            }
            var mesList = compList.ResultObject["maintenanceEngineerings_"].ResultList;
            if (mesList != null) {
                SetEngineerNames(mesList.ToList(), woSite);
            }
        }

        private MaintenanceEngineering HandleMaintenanceEng(CrudOperationData crudoperationData, MaintenanceEngineering me, WorkPackage workpackage, CrudOperationData woData) {
            var status = crudoperationData.GetStringAttribute("status");
            var sendNow = crudoperationData.GetBooleanAttribute("sendnow");

            RequestStatus newStatus;
            Enum.TryParse(status, true, out newStatus);


            if (me.Status.IsSubmitted()) {
                if (!newStatus.IsSubmitted()) {
                    throw new Exception($"Is not possible edit a maintenance engineering request with status '{me.Status}'. Reload the page to get the updated version of this work package.");
                }
                // submited requests are not editable so just return the existing one
                return me;
            }

            me.Engineer = crudoperationData.GetStringAttribute("engineer");
            me.Status = newStatus;

            if (sendNow.HasValue && sendNow.Value) {
                me.SendTime = DateTime.Now.FromServerToMaximo();
                me.SendNow = true;
            } else {
                me.Status = RequestStatus.Scheduled;
                var dateFromJson = Convert.ToDateTime(crudoperationData.GetStringAttribute("sendTime"), new CultureInfo("en-US"));
                me.SendTime = dateFromJson.FromUserToMaximo(SecurityFacade.CurrentUser());
            }
            
            me.Reason = crudoperationData.GetStringAttribute("reason");
            me.Email = MaintenanceEmailService.HandleEmailRecipient(crudoperationData,"email");
            me.Cc= MaintenanceEmailService.HandleEmailRecipient(crudoperationData,"cc");
            me.WorkPackageId = workpackage.Id ?? 0;
            me.GenerateToken();

            me = Dao.Save(me);

            AttachmentsHandler.HandleAttachments(crudoperationData, me.Id ?? 0, AttachmentsRelationship, FilterPrefix, woData);

            return me;
        }

        private static MaintenanceEngineering GetOurCreateMaintenanceEng(CrudOperationData crudoperationData, IList<MaintenanceEngineering> existingMes) {
            var id = crudoperationData.GetIntAttribute("id");
            if (id == null || existingMes == null) {
                return new MaintenanceEngineering();
            }
            var found = existingMes.FirstOrDefault(me => me.Id == id);
            if (found == null) {
                return new MaintenanceEngineering() { Id = id };
            }
            existingMes.Remove(found);
            return found;
        }

        private Dictionary<string, IAssociationOption> SetEngineerNames(List<Dictionary<string, object>> maintenanceEngineerings, string woSite) {
            var options = new Dictionary<string, IAssociationOption>();
            if (maintenanceEngineerings == null || !maintenanceEngineerings.Any()) {
                return options;
            }

            var engineers = maintenanceEngineerings.Select(me => (string)me["engineer"]).ToList();

            var dbOptions = MaxDao.FindByNativeQuery("select personid, displayname, locationsite from person where personid in (:p0)", engineers);
            dbOptions.ForEach(dbOption => {
                var value = dbOption["personid"];
                var label = (string)null;
                dbOption.TryGetValue("displayname", out label);
                var option = new AssociationOption(value, label);

                if (!options.ContainsKey(value)) {
                    options.Add(value, option);
                    return;
                }

                var site = (string)null;
                dbOption.TryGetValue("locationsite", out site);
                if (!string.IsNullOrEmpty(woSite) && woSite.Equals(site)) {
                    options[value] = option;
                }
            });

            maintenanceEngineerings.ForEach(me => {
                var engineer = (string)me["engineer"];
                var option = (IAssociationOption)null;
                options.TryGetValue(engineer, out option);
                if (!string.IsNullOrEmpty(option?.Label)) {
                    me["#engineername"] = option.Label;
                } else {
                    me["#engineername"] = engineer;
                }
            });
            return options;
        }
    }
}