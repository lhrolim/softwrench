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
    class BasePurchaseRequestCrudConnector : CrudConnectorDecorator
    {
        protected AttachmentHandler _attachmentHandler;
        protected ScreenshotHandler _screenshotHandler;

        public BasePurchaseRequestCrudConnector() {
            _attachmentHandler = new AttachmentHandler();
            _screenshotHandler = new ScreenshotHandler(_attachmentHandler);
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData)
        {
            var user = SecurityFacade.CurrentUser();
            var sr = maximoTemplateData.IntegrationObject;

            w.SetValueIfNull(sr, "ACTLABHRS", 0.0);
            w.SetValueIfNull(sr, "ACTLABCOST", 0.0);
            w.SetValueIfNull(sr, "CHANGEDATE", DateTime.Now.FromServerToRightKind(), true);
            w.SetValueIfNull(sr, "CHANGEBY", user.Login);
            w.SetValueIfNull(sr, "REPORTDATE", DateTime.Now.FromServerToRightKind());
            

            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            LongDescriptionHandler.HandleLongDescription(sr, crudData);
            var attachments = crudData.GetRelationship("attachment");
            /*foreach (var attachment in (IEnumerable<CrudOperationData>)attachments)
            {
                HandleAttachmentAndScreenshot(attachment, sr, maximoTemplateData.ApplicationMetadata);
            }*/
            HandlePRLINES(maximoTemplateData,crudData, sr);
            base.BeforeUpdate(maximoTemplateData);
        }

        private void HandlePRLINES(MaximoOperationExecutionContext maximoTemplateData,CrudOperationData crudDataEntity, object sr)
        {
            var prlines = (IEnumerable<CrudOperationData>)crudDataEntity.GetRelationship("prline");
            
            var recordKey = crudDataEntity.Id;
            var user = SecurityFacade.CurrentUser();
            w.CloneArray((IEnumerable<CrudOperationData>)crudDataEntity.GetRelationship("prline"), sr, "PRLINE",
                delegate(object integrationObject, CrudOperationData crudData)
                {
                    if (ReflectionUtil.IsNull(integrationObject, "PRLINENUM"))
                    {
                        w.SetValue(integrationObject, "PRLINENUM", 1);
                    }
                    var enterdate = sr;
                    w.SetValueIfNull(integrationObject, "ENTERDATE", DateTime.Now.FromServerToRightKind());
                    w.SetValueIfNull(integrationObject, "TAX1", 0);
                    w.SetValueIfNull(integrationObject, "TAX2", 0);
                    w.SetValueIfNull(integrationObject, "TAX3", 0);
                    w.SetValueIfNull(integrationObject, "TAX4", 0);
                    w.SetValueIfNull(integrationObject, "TAX5", 0);
                    w.SetValueIfNull(integrationObject, "ISSUE", false);
                    w.SetValueIfNull(integrationObject, "CHARGESTORE", false);
                    w.SetValueIfNull(integrationObject, "RECEIPTREQD", false);
                    w.SetValueIfNull(integrationObject, "LOADEDCOST", 0);
                    w.SetValueIfNull(integrationObject, "PRORATESERVICE", false);
                    w.SetValueIfNull(integrationObject, "CONVERTTORFQ", false);
                    w.SetValueIfNull(integrationObject, "INSPECTIONREQUIRED", false);
                    w.SetValueIfNull(integrationObject, "ISDISTRIBUTED", false);
                    w.SetValueIfNull(integrationObject, "LINECOST", 0);
                    w.SetValueIfNull(integrationObject, "LINETYPE", "ITEM");
                    w.SetValueIfNull(integrationObject, "ENTERBY", user.Login);
                    w.SetValueIfNull(integrationObject, "SITEID",user.SiteId);
                    w.SetValueIfNull(integrationObject, "ORGID", user.OrgId );
                    w.SetValueIfNull(integrationObject, "ENTEREDASTASK",false );
                    w.SetValueIfNull(integrationObject, "CONVERTTOCONTRACT",false );
                    w.SetValueIfNull(integrationObject, "LANGCODE", "EN");
                    w.SetValueIfNull(integrationObject, "CONVERSION", 1);
                    w.SetValueIfNull(integrationObject, "HASID",false );
                    w.SetValueIfNull(integrationObject, "PRLINENUM", 0);
                    w.SetValueIfNull(integrationObject, "MKTPLCITEM",false );
                    w.SetValueIfNull(integrationObject, "TAXEXEMPT",false );
                    w.SetValueIfNull(integrationObject, "CONSIGNMENT",false );
                    w.SetValueIfNull(integrationObject, "ITEMNUM", "test");

                    ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
                });
          
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData)
        {
            var user = SecurityFacade.CurrentUser();
            var sr = maximoTemplateData.IntegrationObject;
            w.SetValue(sr, "ACTLABHRS", 0);
            w.SetValue(sr, "ACTLABCOST", 0);
            w.SetValueIfNull(sr, "REPORTDATE", DateTime.Now.FromServerToRightKind());

            var crudData = (CrudOperationData)maximoTemplateData.OperationData;
            LongDescriptionHandler.HandleLongDescription(sr, crudData);

            HandleAttachmentAndScreenshot(crudData, sr, maximoTemplateData.ApplicationMetadata);
            HandlePRLINES(maximoTemplateData,crudData, sr);
            base.BeforeCreation(maximoTemplateData);
        }

        private void HandleAttachmentAndScreenshot(CrudOperationData data, object maximoObj, ApplicationMetadata applicationMetadata) {

            // Check if Attachment is present
            var attachmentString = data.GetUnMappedAttribute("newattachment");
            var attachmentPath = data.GetUnMappedAttribute("newattachment_path");

            _attachmentHandler.HandleAttachments(maximoObj, attachmentString, attachmentPath, applicationMetadata);


            // Check if Screenshot is present
            var screenshotString = data.GetUnMappedAttribute("newscreenshot");
            var screenshotName = data.GetUnMappedAttribute("newscreenshot_path");

            _screenshotHandler.HandleScreenshot(maximoObj, screenshotString, screenshotName, applicationMetadata);
        }
    }
}
