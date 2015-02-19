using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class BaseIncidentCrudConnector : CrudConnectorDecorator {

        protected AttachmentHandler _attachmentHandler;

        public BaseIncidentCrudConnector() {
            _attachmentHandler = new AttachmentHandler();
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

            // Update or create attachments
            HandleAttachments((CrudOperationData)maximoTemplateData.OperationData, sr, maximoTemplateData.ApplicationMetadata);

            // Update solution 
            HandleSolutions(crudData, sr);
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

            // Update or create attachments
            HandleAttachments(crudData, sr, maximoTemplateData.ApplicationMetadata);
            base.BeforeCreation(maximoTemplateData);
        }

        private void HandleSolutions(CrudOperationData crudDataEntity, object sr) {
            var sympton = crudDataEntity.GetAttribute("symptom_.ldtext");
            var cause = crudDataEntity.GetAttribute("cause_.ldtext");
            var resolution = crudDataEntity.GetAttribute("resolution_.ldtext");
            w.SetValue(sr, "FR1CODE_LONGDESCRIPTION", cause);
            w.SetValue(sr, "FR2CODE_LONGDESCRIPTION", resolution);
            w.SetValue(sr, "PROBLEMCODE_LONGDESCRIPTION", sympton);
        }

        protected virtual void HandleAttachments(CrudOperationData entity, object wo, ApplicationMetadata metadata) {
            var user = SecurityFacade.CurrentUser();

            var attachmentParam = new AttachmentParameters() {
                Data = entity.GetUnMappedAttribute("newattachment"),
                Path = entity.GetUnMappedAttribute("newattachment_path")
            };
            if (!String.IsNullOrWhiteSpace(attachmentParam.Data) && !String.IsNullOrWhiteSpace(attachmentParam.Path)) {
                // Check if file was rich text file - needed to convert it to word document.
                if (attachmentParam.Path.ToLower().EndsWith("rtf")) {
                    var bytes = Convert.FromBase64String(attachmentParam.Data);
                    var decodedString = Encoding.UTF8.GetString(bytes);
                    var compressedScreenshot = CompressionUtil.CompressRtf(decodedString);

                    bytes = Encoding.UTF8.GetBytes(compressedScreenshot);
                    attachmentParam.Data = Convert.ToBase64String(bytes);
                    attachmentParam.Path = attachmentParam.Path.Substring(0, attachmentParam.Path.Length - 3) + "doc";
                }

                _attachmentHandler.HandleAttachments(wo, attachmentParam, metadata);
            }

            var screenshotParam = new AttachmentParameters() {
                Data = entity.GetUnMappedAttribute("newscreenshot"),
                Path = "screen" + DateTime.Now.ToUserTimezone(user).ToString("yyyyMMdd") + ".png"
            };
            if (!String.IsNullOrWhiteSpace(screenshotParam.Data) && !String.IsNullOrWhiteSpace(screenshotParam.Path)) {
                _attachmentHandler.HandleAttachments(wo, attachmentParam, metadata);
            }

            var attachments = entity.GetRelationship("attachment");
            if (attachments != null) {
                // this will only filter new attachments
                foreach (var attachment in ((IEnumerable<CrudOperationData>)attachments).Where(a => a.Id == null)) {
                    var docinfo = (CrudOperationData)attachment.GetRelationship("docinfo");
                    var title = attachment.GetAttribute("document").ToString();
                    var desc = docinfo != null && docinfo.Fields["description"] != null ? docinfo.Fields["description"].ToString() : "";
                    var content = new AttachmentParameters() {
                        Title = title,
                        Data = attachment.GetUnMappedAttribute("newattachment"),
                        Path = attachment.GetUnMappedAttribute("newattachment_path"),
                        Description = desc
                    };

                    if (content.Data != null) {
                         // Check if file was rich text file - needed to convert it to word document.
                        if (content.Path.ToLower().EndsWith("rtf")) {
                            var bytes = Convert.FromBase64String(attachmentParam.Data);
                            var decodedString = Encoding.UTF8.GetString(bytes);
                            var compressedScreenshot = CompressionUtil.CompressRtf(decodedString);

                            bytes = Encoding.UTF8.GetBytes(compressedScreenshot);
                            content.Data = Convert.ToBase64String(bytes);
                            content.Path = attachmentParam.Path.Substring(0, attachmentParam.Path.Length - 3) + "doc";
                        }

                        _attachmentHandler.HandleAttachments(wo, content, metadata);
                    }
                }
            }
        }
    }
}
