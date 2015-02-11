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

            HandleAttachments(crudData, sr, maximoTemplateData.ApplicationMetadata);

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

            HandleAttachments(crudData, sr, maximoTemplateData.ApplicationMetadata);

            base.BeforeCreation(maximoTemplateData);
        }

        protected virtual void HandleAttachments(CrudOperationData entity, object wo, ApplicationMetadata metadata)
        {
            var user = SecurityFacade.CurrentUser();

            var attachmentParam = new AttachmentParameters()
            {
                Data = entity.GetUnMappedAttribute("newattachment"),
                Path = entity.GetUnMappedAttribute("newattachment_path")
            };
            if (!String.IsNullOrWhiteSpace(attachmentParam.Data) && !String.IsNullOrWhiteSpace(attachmentParam.Path))
            {
                _attachmentHandler.HandleAttachments(wo, attachmentParam, metadata);
            }

            var screenshotParam = new AttachmentParameters()
            {
                Data = entity.GetUnMappedAttribute("newscreenshot"),
                Path = "screen" + DateTime.Now.ToUserTimezone(user).ToString("yyyyMMdd") + ".png"
            };
            if (!String.IsNullOrWhiteSpace(screenshotParam.Data) && !String.IsNullOrWhiteSpace(screenshotParam.Path))
            {
                _attachmentHandler.HandleAttachments(wo, attachmentParam, metadata);
            }

            var attachments = entity.GetRelationship("attachment");
            if (attachments != null)
            {
                // this will only filter new attachments
                foreach (var attachment in ((IEnumerable<CrudOperationData>)attachments).Where(a => a.Id == null))
                {
                    var docinfo = attachment.GetRelationship("docinfo");
                    var content = new AttachmentParameters()
                    {
                        Title = attachment.GetAttribute("document").ToString(),
                        Data = attachment.GetUnMappedAttribute("newattachment"),
                        Path = attachment.GetUnMappedAttribute("newattachment_path"),
                        Description = ((CrudOperationData)docinfo).Fields["description"].ToString()
                    };

                    if (content.Data != null)
                    {
                        _attachmentHandler.HandleAttachments(wo, content, metadata);
                    }
                }
            }
        }
    }
}
