using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.ComponentModel.Composition;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using softWrench.sW4.Email;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    public class BaseIncidentCrudConnector : CrudConnectorDecorator {

        [Import]
        public AttachmentHandler AttachmentHandler {get; set; }

        [Import]
        public CommLogHandler CommlogHandler {
            get; set;
        }

        [Import]
        public WorkLogHandler WorkLogHandler {
            get; set;
        }

        [Import]
        public EmailService EmailService {
            get; set;
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var sr = maximoTemplateData.IntegrationObject;
            WorkLogHandler.HandleWorkLogs((CrudOperationData)maximoTemplateData.OperationData, sr);
            w.SetValueIfNull(sr, "ACTLABHRS", 0.0);
            w.SetValueIfNull(sr, "ACTLABCOST", 0.0);
            w.SetValue(sr, "CHANGEDATE", DateTime.Now.FromServerToRightKind(), true);
            w.SetValueIfNull(sr, "CHANGEBY", user.Login);
            w.SetValueIfNull(sr, "REPORTDATE", DateTime.Now.FromServerToRightKind());

            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            //if (crudData.ContainsAttribute("#hasstatuschange")) {
            //    //first let´s 'simply change the status
            //    WsUtil.SetValue(sr, "STATUSIFACE", true);
            //    if (!WsUtil.GetRealValue(sr, "STATUS").Equals("CLOSED")) {
            //        maximoTemplateData.InvokeProxy();
            //        WsUtil.SetValue(sr, "CHANGEBY", user.Login);

            //    } WsUtil.SetValue(sr, "STATUSIFACE", false);
            //}

            // [SWWEB-1194]: not sending status if not chenged
            var hasStatusChange = crudData.GetUnMappedAttribute("#hasstatuschange");
            if (!"true".EqualsIc(hasStatusChange)) {
                ReflectionUtil.SetProperty(sr, "STATUS", null);
            }
            LongDescriptionHandler.HandleLongDescription(sr, crudData);



            // Update or create attachments
            AttachmentHandler.HandleAttachmentAndScreenshot(maximoTemplateData);
            RelatedRecordHandler.HandleRelatedRecords(maximoTemplateData);

            // Update solution 
            SolutionsHandler.HandleSolutions(crudData, sr);
            base.BeforeUpdate(maximoTemplateData);
        }

        public override string ApplicationName() {
            return "incident";
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            var sr = maximoTemplateData.IntegrationObject;
            w.SetValue(sr, "ACTLABHRS", 0);
            w.SetValue(sr, "ACTLABCOST", 0);
            w.SetValueIfNull(sr, "REPORTDATE", DateTime.Now.FromServerToRightKind());

            var crudData = (CrudOperationData)maximoTemplateData.OperationData;
            LongDescriptionHandler.HandleLongDescription(sr, crudData);

            // Update or create attachments
            AttachmentHandler.HandleAttachmentAndScreenshot(maximoTemplateData);

            base.BeforeCreation(maximoTemplateData);
        }

    }
}
