using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
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
            //HandleSolutions(maximoTemplateData, crudData, sr);
            base.BeforeUpdate(maximoTemplateData);
        }

        private void HandleSolutions(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData crudDataEntity, object sr)
        {
            var solutions = (IEnumerable<CrudOperationData>)crudDataEntity.GetRelationship("solution");
            var recordKey = crudDataEntity.Id;
            var user = SecurityFacade.CurrentUser();
            w.CloneArray((IEnumerable<CrudOperationData>)crudDataEntity.GetRelationship("solution"), sr, "SOLUTION",
                delegate(object integrationObject, CrudOperationData crudData)
                {
                    if (ReflectionUtil.IsNull(integrationObject, "SOLUTION"))
                    {
                        w.SetValue(integrationObject, "SOLUTION", -1);
                    }
                    var enterdate = sr;
                    
                    w.SetValueIfNull(integrationObject, "SOLUTIONID", 0);
                    w.SetValueIfNull(integrationObject, "ORGID", user.OrgId);


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

            //HandleAttachmentAndScreenshot(crudData, sr, maximoTemplateData.ApplicationMetadata);

            base.BeforeCreation(maximoTemplateData);
        }


    }
}
