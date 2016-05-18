using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;
using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.email;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Email;


namespace softWrench.sW4.Data.Persistence.WS.Commons {

    public class BaseServiceRequestCrudConnector : CrudConnectorDecorator {

        protected AttachmentHandler _attachmentHandler;
        protected CommLogHandler _commlogHandler;
        protected WorkLogHandler _WorkLogHandler;
        private MaximoHibernateDAO maxHibernate;
        private readonly EmailService _emailService;

        protected MaximoHibernateDAO MaxHibernate
        {
            get
            {
                return maxHibernate;
            }

            set
            {
                maxHibernate = value;
            }
        }

        public BaseServiceRequestCrudConnector() {
            _attachmentHandler = SimpleInjectorGenericFactory.Instance.GetObject<AttachmentHandler>(typeof(AttachmentHandler));
            _commlogHandler = SimpleInjectorGenericFactory.Instance.GetObject<CommLogHandler>(typeof(CommLogHandler));
            _WorkLogHandler = SimpleInjectorGenericFactory.Instance.GetObject<WorkLogHandler>(typeof(WorkLogHandler));
            MaxHibernate = MaximoHibernateDAO.GetInstance();
            _emailService = SimpleInjectorGenericFactory.Instance.GetObject<EmailService>(typeof(EmailService));
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            // Update common fields or transactions prior to maximo operation execution
            CommonTransaction(maximoTemplateData);

            base.BeforeCreation(maximoTemplateData);
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;
            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);

            // COMSW-52 Auto-populate the actual start and end time for a workorder depending on status change
            // TODO: Caching the status field to prevent multiple SQL update.  
            if (crudData.ContainsAttribute("#hasstatuschange")) {
                // Correct combinations of orgid/siteid are null/null, orgid/null, orgid/siteid. You cannot have a siteid paired with a null orgid.
                var maxStatusValues = MaxHibernate.FindByNativeQuery(string.Format("SELECT MAXVALUE FROM SYNONYMDOMAIN WHERE DOMAINID = 'SRSTATUS' AND VALUE = '{0}' AND (SITEID = '{1}' OR SITEID IS null) AND (ORGID = '{2}' OR ORGID IS null) ORDER BY (CASE WHEN ORGID IS NULL THEN 0 ELSE 1 END) DESC, (CASE WHEN SITEID IS NULL THEN 0 ELSE 1 END) DESC", WsUtil.GetRealValue(maximoTemplateData.IntegrationObject, "STATUS"), WsUtil.GetRealValue(maximoTemplateData.IntegrationObject, "SITEID"), WsUtil.GetRealValue(maximoTemplateData.IntegrationObject, "ORGID")), null);
                var maxStatusValue = maxStatusValues.First();
                if (maxStatusValue["MAXVALUE"].Equals("INPROG")) {
                    // We might need to update the client database and cycle the server: update MAXVARS set VARVALUE=1 where VARNAME='SUPPRESSACTCHECK';
                    // Actual date must be in the past - thus we made it a minute behind the current time.   
                    // More info: http://www-01.ibm.com/support/docview.wss?uid=swg1IZ90431
                    w.SetValue(sr, "ACTSTART", DateTime.Now.AddMinutes(-1).FromServerToRightKind());
                } else if (maxStatusValue["MAXVALUE"].EqualsAny("COMP", "CLOSED", "RESOLVED")) {
                    // Actual date must be in the past - thus we made it a minute behind the current time.   
                    w.SetValue(sr, "ACTSTART", DateTime.Now.AddMinutes(-1).FromServerToRightKind());
                    w.SetValue(sr, "ACTFINISH", DateTime.Now.AddMinutes(-1).FromServerToRightKind());
                }
            }


            // Update common fields or transactions prior to maximo operation exection
            CommonTransaction(maximoTemplateData);

            _WorkLogHandler.HandleWorkLogs(crudData, sr);

            _commlogHandler.HandleCommLogs(maximoTemplateData, crudData, sr);

            RelatedRecordHandler.HandleRelatedRecords(maximoTemplateData);

            base.BeforeUpdate(maximoTemplateData);
        }

        public override void AfterUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            if (maximoTemplateData.Properties.ContainsKey("mailObject")) {
                _emailService.SendEmailAsync((EmailData)maximoTemplateData.Properties["mailObject"]);
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
            w.SetValue(sr, "CHANGEDATE", DateTime.Now.FromServerToRightKind(), true);
            w.SetValue(sr, "CHANGEBY", user.Login);
            w.SetValueIfNull(sr, "REPORTDATE", DateTime.Now.FromServerToRightKind());

            // SWWEB-980 Additional logic to change status to queued if owner is selected
            if (w.GetRealValue(sr, "STATUS").Equals("NEW") &&
                (w.GetRealValue(sr, "OWNER") != null || w.GetRealValue(sr, "OWNERGROUP") != null)) {
                w.SetValue(sr, "STATUS", "QUEUED");
            }


            // Update or create new long description 
            LongDescriptionHandler.HandleLongDescription(sr, (CrudOperationData)maximoTemplateData.OperationData);

            HandleServiceAddress(maximoTemplateData);

            _attachmentHandler.HandleAttachmentAndScreenshot(maximoTemplateData);
        }

        private static void HandleServiceAddress(MaximoOperationExecutionContext maximoTemplateData) {
            var data = (CrudOperationData)maximoTemplateData.OperationData;
            var user = SecurityFacade.CurrentUser();
            var saddresscode = data.GetUnMappedAttribute("saddresscode");

            if (saddresscode == null) return;

            var description = data.GetUnMappedAttribute("#tkdesc");
            var formattedaddr = data.GetUnMappedAttribute("#tkformattedaddress");
            var streetnumber = data.GetUnMappedAttribute("#tkstaddrnumber");
            var streetaddr = data.GetUnMappedAttribute("#tkstaddrstreet");
            var streettype = data.GetUnMappedAttribute("#tkstaddrsttype");

            var tkserviceaddress = ReflectionUtil.InstantiateSingleElementFromArray(maximoTemplateData.IntegrationObject, "TKSERVICEADDRESS",true);
            w.SetValueIfNull(tkserviceaddress, "TKSERVICEADDRESSID", -1);
            w.CopyFromRootEntity(maximoTemplateData.IntegrationObject, tkserviceaddress, "ORGID", user.OrgId);
            w.SetValue(tkserviceaddress, "SADDRESSCODE", saddresscode);
            w.SetValue(tkserviceaddress, "DESCRIPTION", description);
            w.SetValue(tkserviceaddress, "FORMATTEDADDRESS", formattedaddr);
            w.SetValue(tkserviceaddress, "STADDRNUMBER", streetnumber);
            w.SetValue(tkserviceaddress, "STADDRSTREET", streetaddr);
            w.SetValue(tkserviceaddress, "STADDRSTTYPE", streettype);
        }


    }
}
