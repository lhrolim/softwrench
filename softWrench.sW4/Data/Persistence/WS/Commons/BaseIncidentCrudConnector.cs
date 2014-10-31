﻿using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Text;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class BaseIncidentCrudConnector : CrudConnectorDecorator {

        protected AttachmentHandler AttachmentHandler;

        public BaseIncidentCrudConnector() {
            AttachmentHandler = new AttachmentHandler();
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var sr = maximoTemplateData.IntegrationObject;
            WorkLogHandler.HandleWorkLogs((CrudOperationData)maximoTemplateData.OperationData, sr);
            w.SetValueIfNull(sr, "ACTLABHRS", 0.0);
            w.SetValueIfNull(sr, "ACTLABCOST", 0.0);
            w.SetValueIfNull(sr, "CHANGEDATE", DateTime.Now.FromServerToRightKind(), true);
            w.SetValueIfNull(sr, "CHANGEBY", user.Login);
            w.SetValueIfNull(sr, "REPORTDATE", DateTime.Now.FromServerToRightKind());

            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            LongDescriptionHandler.HandleLongDescription(sr, crudData);
            var attachments = crudData.GetRelationship("attachment");
            foreach (var attachment in (IEnumerable<CrudOperationData>)attachments) {
                var attachmentString = attachment.GetUnMappedAttribute("newattachment");
                var attachmentPath = attachment.GetUnMappedAttribute("newattachment_path");
                if (attachmentString != null && attachmentPath != null) {
                    AttachmentHandler.HandleAttachments(sr, attachmentString, attachmentPath,
                        maximoTemplateData.ApplicationMetadata);
                }
            }
            HandleSolutions(crudData, sr);
            base.BeforeUpdate(maximoTemplateData);
        }

        private void HandleSolutions(CrudOperationData crudDataEntity, object sr) {
            var sympton = crudDataEntity.GetAttribute("symptom_.ldtext");
            var cause = crudDataEntity.GetAttribute("cause_.ldtext");
            var resolution = crudDataEntity.GetAttribute("resolution_.ldtext");
            w.SetValue(sr, "FR1CODE_LONGDESCRIPTION", cause);
            w.SetValue(sr, "FR2CODE_LONGDESCRIPTION", resolution);
            w.SetValue(sr, "PROBLEMCODE_LONGDESCRIPTION", sympton);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var sr = maximoTemplateData.IntegrationObject;
            w.SetValue(sr, "ACTLABHRS", 0);
            w.SetValue(sr, "ACTLABCOST", 0);
            w.SetValueIfNull(sr, "REPORTDATE", DateTime.Now.FromServerToRightKind());

            var crudData = (CrudOperationData)maximoTemplateData.OperationData;
            LongDescriptionHandler.HandleLongDescription(sr, crudData);

            HandleAttachmentAndScreenshot(crudData, sr, maximoTemplateData.ApplicationMetadata);

            base.BeforeCreation(maximoTemplateData);
        }

        private void HandleAttachmentAndScreenshot(CrudOperationData data, object maximoObj, ApplicationMetadata applicationMetadata) {

            // Check if Attachment is present
            var attachmentString = data.GetUnMappedAttribute("newattachment");
            var attachmentPath = data.GetUnMappedAttribute("newattachment_path");

            if (!String.IsNullOrWhiteSpace(attachmentString) && !String.IsNullOrWhiteSpace(attachmentPath)) {
                AttachmentHandler.HandleAttachments(maximoObj, attachmentString, attachmentPath, applicationMetadata);
            }

            // Check if Screenshot is present
            var screenshotString = data.GetUnMappedAttribute("newscreenshot");
            var screenshotName = data.GetUnMappedAttribute("newscreenshot_path");

            if (!String.IsNullOrWhiteSpace(screenshotString) && !String.IsNullOrWhiteSpace(screenshotName)) {

                if (screenshotName.ToLower().EndsWith("rtf")) {
                    var bytes = Convert.FromBase64String(screenshotString);
                    var decodedString = Encoding.UTF8.GetString(bytes);
                    var compressedScreenshot = CompressionUtil.CompressRtf(decodedString);

                    bytes = Encoding.UTF8.GetBytes(compressedScreenshot);
                    screenshotString = Convert.ToBase64String(bytes);
                    screenshotName = screenshotName.Substring(0, screenshotName.Length - 3) + "doc";
                }

                AttachmentHandler.HandleAttachments(maximoObj, screenshotString, screenshotName, applicationMetadata);
            }
        }
    }
}
