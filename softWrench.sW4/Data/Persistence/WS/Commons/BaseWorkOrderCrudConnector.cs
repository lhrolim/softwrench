using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using r = softWrench.sW4.Util.ReflectionUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class BaseWorkOrderCrudConnector : CrudConnectorDecorator {
        //        private const string _dbwostatusKey = "dbwostatus";
        //        private const string _oldlongdescriptionKey = "oldlongdescriptionKey";
        //        private const string _newlongdescriptionKey = "newlongdescriptionKey";
        //        private const string _notFoundLog = "{0} {1} not found. Impossible to generate FollowUp Workorder";

        protected AttachmentHandler _attachmentHandler;

        public BaseWorkOrderCrudConnector() {
            _attachmentHandler = new AttachmentHandler();
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            CommonTransaction(maximoTemplateData);
            base.BeforeUpdate(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            CommonTransaction(maximoTemplateData);
            base.BeforeCreation(maximoTemplateData);
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
            
            WorkLogHandler.HandleWorkLogs((CrudOperationData)maximoTemplateData.OperationData, wo);
            HandleMaterials((CrudOperationData)maximoTemplateData.OperationData, wo);
            HandleLabors((CrudOperationData)maximoTemplateData.OperationData, wo);
            HandleAttachments((CrudOperationData)maximoTemplateData.OperationData, wo, maximoTemplateData.ApplicationMetadata);
        }

        protected virtual void HandleLabors(CrudOperationData entity, object maximoWo) {
            WsUtil.CloneArray((IEnumerable<CrudOperationData>)entity.GetRelationship("labtrans"), maximoWo, "LABTRANS",
                delegate(object integrationObject, CrudOperationData crudData) {
                    if (ReflectionUtil.IsNull(integrationObject, "LABTRANSID")) {
                        WsUtil.SetValue(integrationObject, "LABTRANSID", -1);
                    }
                });
        }

        protected virtual void HandleMaterials(CrudOperationData entity, object wo) {
            // Use to obtain security information from current user
            var user = SecurityFacade.CurrentUser();

            // Workorder id used for data association
            var recordKey = entity.Id;

            // Filter work order materials for any new entries
            var Materials = (IEnumerable<CrudOperationData>)entity.GetRelationship("matusetrans");
            var newMaterials = Materials.Where(r => r.GetAttribute("matusetransid") == null);

            // Convert collection into array, if any are available
            var crudOperationData = newMaterials as CrudOperationData[] ?? newMaterials.ToArray();
            
            WsUtil.CloneArray(crudOperationData, wo, "MATUSETRANS", delegate(object integrationObject, CrudOperationData crudData) {
                
                WsUtil.SetValueIfNull(integrationObject, "QTYREQUESTED", 0);
                WsUtil.SetValueIfNull(integrationObject, "UNITCOST", 0);
                WsUtil.SetValueIfNull(integrationObject, "CURBAL", 0);

                var quantity = (double)WsUtil.GetRealValue(integrationObject, "QTYREQUESTED");
                var unitcost = (double)WsUtil.GetRealValue(integrationObject, "UNITCOST");
                var curbal   = (double)WsUtil.GetRealValue(integrationObject, "CURBAL"); 
                var transaction = WsUtil.GetRealValue(integrationObject, "LINETYPE");

                WsUtil.SetValue(integrationObject, "TRANSDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValue(integrationObject, "ACTUALDATE", DateTime.Now.FromServerToRightKind(), true);
                WsUtil.SetValue(integrationObject, "QUANTITY", -1 * quantity);
                WsUtil.SetValueIfNull(integrationObject, "PHYSCNT", 0);
                WsUtil.SetValueIfNull(integrationObject, "UNITCOST", 0);
                WsUtil.SetValue(integrationObject, "ACTUALCOST", unitcost);
                WsUtil.SetValueIfNull(integrationObject, "CONVERSION", 1);
                WsUtil.SetValue(integrationObject, "ENTERBY", user.Login);
                WsUtil.SetValue(integrationObject, "ROLLUP", 0);
                WsUtil.SetValue(integrationObject, "LINECOST", quantity * unitcost);
                WsUtil.SetValue(integrationObject, "CURRENCYCODE", "USD");
                WsUtil.SetValue(integrationObject, "CURRENCYUNITCOST", unitcost);
                WsUtil.SetValue(integrationObject, "CURRENCYLINECOST", quantity * unitcost);
                WsUtil.SetValueIfNull(integrationObject, "DESCRIPTION", "");
                WsUtil.SetValueIfNull(integrationObject, "EXCHANGERATE", 1);
                WsUtil.SetValueIfNull(integrationObject, "EXCHANGERATE2", 0.0000000);
                WsUtil.SetValueIfNull(integrationObject, "LINECOST2", 0.00);
                WsUtil.SetValue(integrationObject, "ORGID", user.OrgId);
                WsUtil.SetValue(integrationObject, "SITEID", user.SiteId);
                WsUtil.SetValue(integrationObject, "REFWO", recordKey);
                WsUtil.SetValue(integrationObject, "CONDRATE", 100);
                WsUtil.SetValue(integrationObject, "ENTEREDASTASK", 0); 

                WsUtil.SetValueIfNull(integrationObject, "ISSUETYPE", "ISSUE");
                WsUtil.SetValue(integrationObject, "MATUSETRANSID", -1);

                ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
            });
        }

        protected virtual void HandleAttachments(CrudOperationData entity, object wo, ApplicationMetadata metadata) {
            var user = SecurityFacade.CurrentUser();

            var attachmentString = entity.GetUnMappedAttribute("newattachment");
            var attachmentPath = entity.GetUnMappedAttribute("newattachment_path");
            if (!String.IsNullOrWhiteSpace(attachmentString) && !String.IsNullOrWhiteSpace(attachmentPath)) {
                _attachmentHandler.HandleAttachments(wo, attachmentString, attachmentPath, metadata);
            }
            var screenshotString = entity.GetUnMappedAttribute("newscreenshot");
            var screenshotName = "screen" + DateTime.Now.ToUserTimezone(user).ToString("yyyyMMdd") + ".png";
            if (!String.IsNullOrWhiteSpace(screenshotString) && !String.IsNullOrWhiteSpace(screenshotName)) {
                _attachmentHandler.HandleAttachments(wo, screenshotString, screenshotName, metadata);
            }

            var attachments = entity.GetRelationship("attachment");
            if (attachments != null) {
                foreach (var attachment in (IEnumerable<CrudOperationData>)attachments) {
                    attachmentString = attachment.GetUnMappedAttribute("newattachment");
                    attachmentPath = (string)(attachment.GetUnMappedAttribute("newattachment_path") ?? attachment.GetAttribute("document"));
                    if (attachmentString != null && attachmentPath != null) {
                        _attachmentHandler.HandleAttachments(wo, attachmentString, attachmentPath, metadata);
                    }
                }
            }
        }
    }
}
