using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.email;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Email;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Persistence.WS.Applications.Workorder {

    class BaseWorkOrderCrudConnector : CrudConnectorDecorator {
        //        private const string _dbwostatusKey = "dbwostatus";
        //        private const string _oldlongdescriptionKey = "oldlongdescriptionKey";
        //        private const string _newlongdescriptionKey = "newlongdescriptionKey";
        //        private const string _notFoundLog = "{0} {1} not found. Impossible to generate FollowUp Workorder";

        protected AttachmentHandler AttachmentHandler {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<AttachmentHandler>(typeof(AttachmentHandler));
            }
        }

        protected CommLogHandler CommlogHandler {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<CommLogHandler>(typeof(CommLogHandler));
            }
        }

        protected EmailService EmailService {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<EmailService>(typeof(EmailService));
            }
        }

        protected LabTransHandler LabTransHandler {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<LabTransHandler>(typeof(LabTransHandler));
            }
        }

        protected WorkLogHandler WorkLogHandler {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<WorkLogHandler>(typeof(WorkLogHandler));
            }
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var wo = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            CommonTransaction(maximoTemplateData);
            if (crudData.ContainsAttribute("#hasstatuschange")) {
                //first let´s 'simply change the status --> this is needed to create a WOSTATUS TX entry
                WsUtil.SetValue(wo, "STATUSIFACE", true);
                WsUtil.SetValue(wo, "CHANGEBY", user.Login.ToUpper());
                var status = WsUtil.GetRealValue(wo, "STATUS") as string;
                if (!status.EqualsAny("CAN", "CLOSE", "COMP")) {
                    //TODO: review these status later, whether it would make sense to abort TX here
                    maximoTemplateData.InvokeProxy();
                }
                WsUtil.SetValue(wo, "STATUSIFACE", false);
            }

            // COMSW-52 Auto-populate the actual start and end time for a workorder depending on status change
            // TODO: Caching the status field to prevent multiple SQL update.  
            if (crudData.ContainsAttribute("#hasstatuschange")) {
                // Correct combinations of orgid/siteid are null/null, orgid/null, orgid/siteid. You cannot have a siteid paired with a null orgid.
                var maxStatusValues = MaximoHibernateDAO.GetInstance().FindByNativeQuery(String.Format("SELECT MAXVALUE FROM SYNONYMDOMAIN WHERE DOMAINID = 'WOSTATUS' AND VALUE = '{0}' AND (SITEID = '{1}' OR SITEID IS null) AND (ORGID = '{2}' OR ORGID IS null) ORDER BY (CASE WHEN ORGID IS NULL THEN 0 ELSE 1 END) DESC, (CASE WHEN SITEID IS NULL THEN 0 ELSE 1 END) DESC", WsUtil.GetRealValue(maximoTemplateData.IntegrationObject, "STATUS"), WsUtil.GetRealValue(maximoTemplateData.IntegrationObject, "SITEID"), WsUtil.GetRealValue(maximoTemplateData.IntegrationObject, "ORGID")), null);
                var maxStatusValue = maxStatusValues.First();
                if (maxStatusValue.ContainsKey("MAXVALUE") && maxStatusValue["MAXVALUE"].Equals("INPRG")) {
                    // We might need to update the client database and cycle the server: update MAXVARS set VARVALUE=1 where VARNAME='SUPPRESSACTCHECK';
                    // Actual date must be in the past - thus we made it a minute behind the current time.   
                    // More info: http://www-01.ibm.com/support/docview.wss?uid=swg1IZ90431
                    WsUtil.SetValueIfNull(wo, "ACTSTART", DateTime.Now.AddMinutes(-1).FromServerToRightKind());
                } else if (maxStatusValue.ContainsKey("MAXVALUE") && maxStatusValue["MAXVALUE"].Equals("COMP")) {
                    // Actual date must be in the past - thus we made it a minute behind the current time.   
                    WsUtil.SetValue(wo, "ACTSTART", DateTime.Now.AddMinutes(-1).FromServerToRightKind());
                    WsUtil.SetValue(wo, "ACTFINISH", DateTime.Now.AddMinutes(-1).FromServerToRightKind());
                }
            }



            // This will prevent multiple action on these items
            WorkLogHandler.HandleWorkLogs(crudData, wo);
            MultiAssetLocciHandler.HandleMultiAssetLoccis(crudData, wo);
            HandleMaterials(crudData, wo);
            LabTransHandler.HandleLabors(crudData, wo);
            ToolsHandler.HandleWoTools(crudData, wo);
            // Update or create related records
            RelatedRecordHandler.HandleRelatedRecords(maximoTemplateData);
            // Update or create attachments
            AttachmentHandler.HandleAttachmentAndScreenshot(maximoTemplateData);
            // Update or create commlogs
            CommlogHandler.HandleCommLogs(maximoTemplateData, crudData, wo);

            base.BeforeUpdate(maximoTemplateData);
        }

        public override void AfterUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            if (maximoTemplateData.Properties.ContainsKey("mailObject")) {
                EmailService.SendEmailAsync((EmailData)maximoTemplateData.Properties["mailObject"]);
            }

            //TODO: Delete the failed commlog entry or marked as failed : Input from JB needed 
            base.AfterUpdate(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            CommonTransaction(maximoTemplateData);
            // Update or create attachments
            AttachmentHandler.HandleAttachmentAndScreenshot(maximoTemplateData);
            base.BeforeCreation(maximoTemplateData);
        }

        public override void AfterCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var svcaddressExists = ((CrudOperationData)maximoTemplateData.OperationData).GetAttribute("#haswoaddresschange") != null;
            if (!svcaddressExists) {
                //if no service address is passed we don´t need to perform any update afterwards
                return;
            }

            base.AfterUpdate(maximoTemplateData);

            ((CrudOperationData)maximoTemplateData.OperationData).Fields["wonum"] = maximoTemplateData.ResultObject.UserId;
            maximoTemplateData.OperationData.OperationType = Internal.OperationType.AddChange;

            // Resubmitting MIF for ServiceAddress Update
            ConnectorEngine.Update((CrudOperationData)maximoTemplateData.OperationData);
        }



        private void CommonTransaction(MaximoOperationExecutionContext maximoTemplateData) {
            var wo = maximoTemplateData.IntegrationObject;
            WsUtil.SetValueIfNull(wo, "ESTDUR", 0);
            WsUtil.SetValueIfNull(wo, "ESTLABHRS", 0);
            WsUtil.SetValueIfNull(wo, "ESTMATCOST", 0);

            WsUtil.SetValueIfNull(wo, "ESTINTLABHRS", 0);
            WsUtil.SetValueIfNull(wo, "ESTINTLABCOST", 0);

            WsUtil.SetValueIfNull(wo, "ESTATAPPRLABHRS", 0);
            WsUtil.SetValueIfNull(wo, "ESTATAPPRMATCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTATAPPRLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTATAPPRTOOLCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTATAPPRSERVCOST", 0);

            WsUtil.SetValueIfNull(wo, "ESTAPPRLABHRS", 0);
            WsUtil.SetValueIfNull(wo, "ESTAPPRMATCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTAPPRLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "WOCLASS", "WORKORDER");
            WsUtil.SetValueIfNull(wo, "ESTLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTTOOLCOST", 0);
            WsUtil.SetValueIfNull(wo, "ESTSERVCOST", 0);
            WsUtil.SetValueIfNull(wo, "ACTLABHRS", 0);
            WsUtil.SetValueIfNull(wo, "ACTLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "ACTLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "ACTSERVCOST", 0);
            WsUtil.SetValueIfNull(wo, "ACTMATCOST", 0);
            WsUtil.SetValueIfNull(wo, "ACTTOOLCOST", 0);
            WsUtil.SetValueIfNull(wo, "OUTLABCOST", 0);
            WsUtil.SetValueIfNull(wo, "OUTMATCOST", 0);
            WsUtil.SetValueIfNull(wo, "OUTTOOLCOST", 0);
            WsUtil.SetValueIfNull(wo, "OUTSERVCOST", 0);

            LongDescriptionHandler.HandleLongDescription(maximoTemplateData.IntegrationObject, (CrudOperationData)maximoTemplateData.OperationData);
            ServiceAddressHandler.HandleServiceAddressForWo((CrudOperationData)maximoTemplateData.OperationData, maximoTemplateData.IntegrationObject);
        }





        protected virtual void HandleMaterials(CrudOperationData entity, object wo) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Workorder id used for data association
            var recordKey = entity.UserId;

            // Filter work order materials for any new entries where matusetransid is null
            var Materials = (IEnumerable<CrudOperationData>)entity.GetRelationship("matusetrans");
            var newMaterials = Materials.Where(r => r.GetAttribute("matusetransid") == null);

            // Convert collection into array, if any are available
            var crudOperationData = newMaterials as CrudOperationData[] ?? newMaterials.ToArray();

            if (crudOperationData.Length > 1) {
                crudOperationData = crudOperationData.Skip(crudOperationData.Length - 1).ToArray();
            }

            WsUtil.CloneArray(crudOperationData, wo, "MATUSETRANS", delegate (object integrationObject, CrudOperationData crudData) {

                WsUtil.SetValueIfNull(integrationObject, "QTYREQUESTED", 0);
                WsUtil.SetValueIfNull(integrationObject, "UNITCOST", 0);

                var itemtype = WsUtil.GetRealValue(integrationObject, "LINETYPE").ToString();
                var quantity = (double)WsUtil.GetRealValue(integrationObject, "QTYREQUESTED");
                var unitcost = (double)WsUtil.GetRealValue(integrationObject, "UNITCOST");

                // Sparepart's are items, the linetype SPAREPART is only needed
                // for front end and must be converted to a valid type for
                // submission to the MIF
                if (itemtype == "SPAREPART") {
                    itemtype = "ITEM";
                    WsUtil.SetValue(integrationObject, "LINETYPE", itemtype);
                }

                if (itemtype.Equals("ITEM")) {
                    WsUtil.SetValue(integrationObject, "DESCRIPTION", crudData.UnmappedAttributes["#description"]);
                }

                WsUtil.SetValueIfNull(integrationObject, "UNITCOST", 0.0);
                WsUtil.SetValueIfNull(integrationObject, "DESCRIPTION", "");
                WsUtil.SetValueIfNull(integrationObject, "CONVERSION", 1.0);
                
                var now = DateTime.Now.FromServerToRightKind();
                WsUtil.SetValue(integrationObject, "TRANSDATE", now, true);
                WsUtil.SetValue(integrationObject, "ACTUALDATE", now, true);

                WsUtil.SetValue(integrationObject, "ACTUALCOST", unitcost);
                WsUtil.SetValue(integrationObject, "QUANTITY", -1 * quantity);
                WsUtil.SetValue(integrationObject, "LINECOST", quantity * unitcost);
                WsUtil.SetValue(integrationObject, "ENTERBY", user.Login);

                WsUtil.SetValue(integrationObject, "ORGID", entity.GetAttribute("orgid"));
                WsUtil.SetValue(integrationObject, "SITEID", entity.GetAttribute("siteid"));
                WsUtil.SetValue(integrationObject, "TOSITEID", entity.GetAttribute("siteid"));
                WsUtil.SetValue(integrationObject, "REFWO", recordKey);

                WsUtil.SetValueIfNull(integrationObject, "ISSUETYPE", "ISSUE");
                WsUtil.SetValue(integrationObject, "MATUSETRANSID", -1);

                ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
            });
        }
    }
}
