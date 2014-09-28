using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Text;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons
{
    class BaseChangeRequestCrudConnector : CrudConnectorDecorator
    {
        protected AttachmentHandler _attachmentHandler;

        public BaseChangeRequestCrudConnector() {
            _attachmentHandler = new AttachmentHandler();
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var sr = maximoTemplateData.IntegrationObject;
            
            w.SetValueIfNull(sr, "ACTLABHRS", 0.0);
            w.SetValueIfNull(sr, "ACTLABCOST", 0.0);
            w.SetValueIfNull(sr, "CHANGEDATE", DateTime.Now.FromServerToRightKind(), true);
            w.SetValueIfNull(sr, "CHANGEBY", user.Login);
            w.SetValueIfNull(sr, "REPORTDATE", DateTime.Now.FromServerToRightKind());
            WorkLogHandler.HandleWorkLogs((CrudOperationData)maximoTemplateData.OperationData, sr);

            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            LongDescriptionHandler.HandleLongDescription(sr, crudData);
            var attachments = crudData.GetRelationship("attachment");
            foreach (var attachment in (IEnumerable<CrudOperationData>)attachments) {
                HandleAttachmentAndScreenshot(attachment, sr, maximoTemplateData.ApplicationMetadata);
            }
            base.BeforeUpdate(maximoTemplateData);
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
                _attachmentHandler.HandleAttachments(maximoObj, attachmentString, attachmentPath, applicationMetadata);
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

                _attachmentHandler.HandleAttachments(maximoObj, screenshotString, screenshotName, applicationMetadata);
            }
        }
    }
}
