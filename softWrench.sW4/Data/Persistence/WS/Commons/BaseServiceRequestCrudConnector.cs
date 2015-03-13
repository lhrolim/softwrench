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
using cts.commons.simpleinjector;
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
            var user = SecurityFacade.CurrentUser();
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            //if (crudData.ContainsAttribute("#hasstatuschange")){
            //    //first let´s 'simply change the status
            //    WsUtil.SetValue(sr, "STATUSIFACE", true);
            //    if (!WsUtil.GetRealValue(sr, "STATUS").Equals("CLOSED")){
            //        maximoTemplateData.InvokeProxy();

            //     //Duplication of code in CommonTransaction()
            //        WsUtil.SetValue(sr, "CHANGEBY", user.Login);

            //    } WsUtil.SetValue(sr, "STATUSIFACE", false);
            //}

            // Update common fields or transactions prior to maximo operation exection
            CommonTransaction(maximoTemplateData);

            var mailObject = maximoTemplateData.Properties;

            WorkLogHandler.HandleWorkLogs(crudData, sr);
            CommLogHandler.HandleCommLogs(maximoTemplateData, crudData, sr);

            HandleServiceAddress(maximoTemplateData);

            _attachmentHandler.HandleAttachmentAndScreenshot(maximoTemplateData);

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
            w.SetValue(sr, "CHANGEBY", user.Login);
            w.SetValueIfNull(sr, "REPORTDATE", DateTime.Now.FromServerToRightKind());

            // SWWEB-980 Additional logic to change status to queued if owner is selected
            if (WsUtil.GetRealValue(sr, "STATUS").Equals("NEW") && (WsUtil.GetRealValue(sr, "OWNER") != null || WsUtil.GetRealValue(sr, "OWNERGROUP") != null))
                WsUtil.SetValue(sr, "STATUS", "QUEUED"); 

            // Update or create new long description 
            LongDescriptionHandler.HandleLongDescription(sr, ((CrudOperationData)maximoTemplateData.OperationData));
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
    }
}
