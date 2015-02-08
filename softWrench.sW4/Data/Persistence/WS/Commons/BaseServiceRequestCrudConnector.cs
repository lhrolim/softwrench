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
using System.Linq;
using System.Net.Mail;
using System.Net;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Email;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Maximo;


namespace softWrench.sW4.Data.Persistence.WS.Commons {

    class BaseServiceRequestCrudConnector : CrudConnectorDecorator {

        protected AttachmentHandler _attachmentHandler;

        private readonly EmailService _emailService;

        public BaseServiceRequestCrudConnector() {
            _attachmentHandler = new AttachmentHandler();
            _emailService = SimpleInjectorGenericFactory.Instance.GetObject<EmailService>(typeof(EmailService));
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            // Update common fields or transactions prior to maximo operation exection
            CommonTransaction(maximoTemplateData);

            // Attempt to get attachment for new SR
            // HandleAttachmentAndScreenshot((CrudOperationData)maximoTemplateData.OperationData, maximoTemplateData.IntegrationObject, maximoTemplateData.ApplicationMetadata);

            base.BeforeCreation(maximoTemplateData);
        }

        public override void AfterCreation(MaximoOperationExecutionContext maximoTemplateData) {
            base.AfterUpdate(maximoTemplateData);
            maximoTemplateData.OperationData.Id = maximoTemplateData.ResultObject.Id;
            maximoTemplateData.OperationData.OperationType = Internal.OperationType.AddChange;

            // Resubmitting MIF for ServiceAddress Update
            ConnectorEngine.Update((CrudOperationData)maximoTemplateData.OperationData);
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            if (crudData.ContainsAttribute("#hasstatuschange")){
                //first let´s 'simply change the status
                WsUtil.SetValue(sr, "STATUSIFACE", true);
                if (!WsUtil.GetRealValue(sr, "STATUS").Equals("CLOSED")){
                    maximoTemplateData.InvokeProxy();

                } WsUtil.SetValue(sr, "STATUSIFACE", false);
            }

            // Update common fields or transactions prior to maximo operation exection
            CommonTransaction(maximoTemplateData);

            var mailObject = maximoTemplateData.Properties;

            WorkLogHandler.HandleWorkLogs(crudData, sr);
            CommLogHandler.HandleCommLogs(maximoTemplateData, crudData, sr);

            HandleServiceAddress(maximoTemplateData);
            HandleAttachmentAndScreenshot((CrudOperationData)maximoTemplateData.OperationData, sr, maximoTemplateData.ApplicationMetadata);

            base.BeforeUpdate(maximoTemplateData);
        }

        public override void AfterUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            if (maximoTemplateData.Properties.ContainsKey("mailObject")) {
                _emailService.SendEmail((EmailService.EmailData)maximoTemplateData.Properties["mailObject"]);
            }

            //TODO: Delete the failed commlog entry or marked as failed : Input from JB needed 
            base.AfterUpdate(maximoTemplateData);
        }


        private void CommonTransaction(MaximoOperationExecutionContext maximoTemplateData) {
            // Get current username that trigger the transaction
            var user = SecurityFacade.CurrentUser();

            var sr = maximoTemplateData.IntegrationObject;
            w.SetValueIfNull(sr, "ACTLABHRS", 0.0);
            w.SetValueIfNull(sr, "ACTLABCOST", 0.0);
            w.SetValueIfNull(sr, "CHANGEDATE", DateTime.Now.FromServerToRightKind(), true);
            w.SetValueIfNull(sr, "CHANGEBY", user.Login);
            w.SetValueIfNull(sr, "REPORTDATE", DateTime.Now.FromServerToRightKind());

            // Update or create new long description 
            LongDescriptionHandler.HandleLongDescription(sr, ((CrudOperationData)maximoTemplateData.OperationData));

            // Update or create attachments
            // HandleAttachmentAndScreenshot((CrudOperationData)maximoTemplateData.OperationData, sr, maximoTemplateData.ApplicationMetadata);
        }

        private bool HandleServiceAddress(MaximoOperationExecutionContext maximoTemplateData) {
            var data = (CrudOperationData)maximoTemplateData.OperationData;
            var user = SecurityFacade.CurrentUser();
            var saddresscode = data.GetUnMappedAttribute("saddresscode");

            if (saddresscode == null) return false;

            var description = data.GetUnMappedAttribute("#tkdesc");
            var formattedaddr = data.GetUnMappedAttribute("#tkformattedaddress");
            var streetnumber = data.GetUnMappedAttribute("#tkstaddrnumber");
            var streetaddr = data.GetUnMappedAttribute("#tkstaddrstreet");
            var streettype = data.GetUnMappedAttribute("#tkstaddrsttype");

            var tkserviceaddress = ReflectionUtil.InstantiateSingleElementFromArray(maximoTemplateData.IntegrationObject, "TKSERVICEADDRESS");
            w.SetValueIfNull(tkserviceaddress, "TKSERVICEADDRESSID", -1);
            w.SetValue(tkserviceaddress, "ORGID", user.OrgId);
            w.SetValue(tkserviceaddress, "SADDRESSCODE", saddresscode);
            w.SetValue(tkserviceaddress, "DESCRIPTION", description);
            w.SetValue(tkserviceaddress, "FORMATTEDADDRESS", formattedaddr);
            w.SetValue(tkserviceaddress, "STADDRNUMBER", streetnumber);
            w.SetValue(tkserviceaddress, "STADDRSTREET", streetaddr);
            w.SetValue(tkserviceaddress, "STADDRSTTYPE", streettype);

            return true;
        }

        private void HandleAttachmentAndScreenshot(CrudOperationData entity, object sr, ApplicationMetadata metadata) {
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

                _attachmentHandler.HandleAttachments(sr, attachmentParam, metadata);
            }

            var screenshotParam = new AttachmentParameters() {
                Data = entity.GetUnMappedAttribute("newscreenshot"),
                Path = "screen" + DateTime.Now.ToUserTimezone(user).ToString("yyyyMMdd") + ".png"
            };
            if (!String.IsNullOrWhiteSpace(screenshotParam.Data) && !String.IsNullOrWhiteSpace(screenshotParam.Path)) {
                _attachmentHandler.HandleAttachments(sr, attachmentParam, metadata);
            }

            var attachments = entity.GetRelationship("attachment");
            if (attachments != null) {
                // this will only filter new attachments
                foreach (var attachment in ((IEnumerable<CrudOperationData>)attachments).Where(a => a.Id == null)) {
                    var docinfo = attachment.GetRelationship("docinfo");
                    var content = new AttachmentParameters() {
                        Title = attachment.GetAttribute("document").ToString(),
                        Data = attachment.GetUnMappedAttribute("newattachment"),
                        Path = attachment.GetUnMappedAttribute("newattachment_path"),
                        Description = ((CrudOperationData)docinfo).Fields["description"].ToString()
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

                        _attachmentHandler.HandleAttachments(sr, content, metadata);
                    }
                }
            }
        }
    }
}
