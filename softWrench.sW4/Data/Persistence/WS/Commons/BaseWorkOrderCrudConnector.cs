using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;

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
            CommonsBefore(maximoTemplateData);
            base.BeforeUpdate(maximoTemplateData);
        }


        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            CommonsBefore(maximoTemplateData);
            base.BeforeCreation(maximoTemplateData);
        }

        private void CommonsBefore(MaximoOperationExecutionContext maximoTemplateData) {
            var entity = (CrudOperationData)maximoTemplateData.OperationData;
            LongDescriptionHandler.HandleLongDescription(maximoTemplateData.IntegrationObject, entity);
            var maximoWo = maximoTemplateData.IntegrationObject;
            var metadata = maximoTemplateData.ApplicationMetadata;
            SetDefaultValues(maximoWo);
            WorkLogHandler.HandleWorkLogs(entity, maximoWo);
            HandleMaterials(maximoTemplateData, entity, maximoWo);
            HandleLabors(entity, maximoWo);
            HandleAttachments(entity, maximoWo, metadata);
        }

        private static void SetDefaultValues(object maximoWo) {
            WsUtil.SetValueIfNull(maximoWo, "ESTDUR", 0);
            WsUtil.SetValueIfNull(maximoWo, "ESTLABHRS", 0);
            WsUtil.SetValueIfNull(maximoWo, "ESTMATCOST", 0);

            WsUtil.SetValueIfNull(maximoWo, "ESTINTLABHRS", 0);
            WsUtil.SetValueIfNull(maximoWo, "ESTINTLABCOST", 0);

            WsUtil.SetValueIfNull(maximoWo, "ESTATAPPRLABHRS", 0);
            WsUtil.SetValueIfNull(maximoWo, "ESTATAPPRMATCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "ESTATAPPRLABCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "ESTATAPPRTOOLCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "ESTATAPPRSERVCOST", 0);

            WsUtil.SetValueIfNull(maximoWo, "ESTAPPRLABHRS", 0);
            WsUtil.SetValueIfNull(maximoWo, "ESTAPPRMATCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "ESTAPPRLABCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "WOCLASS", "WORKORDER");
            WsUtil.SetValueIfNull(maximoWo, "ESTLABCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "ESTTOOLCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "ESTSERVCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "ACTLABHRS", 0);
            WsUtil.SetValueIfNull(maximoWo, "ACTLABCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "ACTLABCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "ACTSERVCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "ACTMATCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "ACTTOOLCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "OUTLABCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "OUTMATCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "OUTTOOLCOST", 0);
            WsUtil.SetValueIfNull(maximoWo, "OUTSERVCOST", 0);
            
        }

        protected virtual void HandleLabors(CrudOperationData entity, object maximoWo) {
            WsUtil.CloneArray((IEnumerable<CrudOperationData>)entity.GetRelationship("labtrans"), maximoWo, "LABTRANS",
                delegate(object integrationObject, CrudOperationData crudData) {
                    if (ReflectionUtil.IsNull(integrationObject, "LABTRANSID")) {
                        WsUtil.SetValue(integrationObject, "LABTRANSID", -1);
                    }
                });
        }

        protected virtual void HandleMaterials(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData entity, object wo) {
            var materials = (IEnumerable<CrudOperationData>)entity.GetRelationship("matusetrans");
            var newMaterials = materials.Where(r => r.GetAttribute("matusetransid") == null);
            var recordKey = entity.Id;
            var user = SecurityFacade.CurrentUser();
            WsUtil.CloneArray(newMaterials, wo, "MATUSETRANS", delegate(object integrationObject, CrudOperationData crudData) {
                var qtyRequested = ReflectionUtil.GetProperty(integrationObject, "QTYREQUESTED");
                if (qtyRequested == null) {
                    WsUtil.SetValue(integrationObject, "QTYREQUESTED", 0);
                }
                var realValue = (double)WsUtil.GetRealValue(integrationObject, "QTYREQUESTED");
                WsUtil.SetValue(integrationObject, "QUANTITY", -1 * realValue);
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
