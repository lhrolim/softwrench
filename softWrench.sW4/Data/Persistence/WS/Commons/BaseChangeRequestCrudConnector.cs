using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
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
            
            w.SetValueIfNull(sr, "CHANGEDATE", DateTime.Now.FromServerToRightKind(), true);
            w.SetValueIfNull(sr, "CHANGEBY", user.Login);

            WorkLogHandler.HandleWorkLogs((CrudOperationData)maximoTemplateData.OperationData, sr);

            CommonTransaction(maximoTemplateData);

            base.BeforeUpdate(maximoTemplateData);
        }

        public override void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            CommonTransaction(maximoTemplateData);           
            base.BeforeCreation(maximoTemplateData);
        }

        private void CommonTransaction(MaximoOperationExecutionContext maximoTemplateData) {
            var user = SecurityFacade.CurrentUser();
            var sr = maximoTemplateData.IntegrationObject;

            w.SetValueIfNull(sr, "ACTLABHRS", 0.0);
            w.SetValueIfNull(sr, "ACTLABCOST", 0.0);
            w.SetValueIfNull(sr, "REPORTDATE", DateTime.Now.FromServerToRightKind());

            var crudData = (CrudOperationData)maximoTemplateData.OperationData;
            LongDescriptionHandler.HandleLongDescription(sr, crudData);

            _attachmentHandler.HandleAttachmentAndScreenshot(maximoTemplateData);
        }
    }
}
