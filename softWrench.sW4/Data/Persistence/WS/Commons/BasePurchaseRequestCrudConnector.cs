using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.Commons {
    class BasePurchaseRequestCrudConnector : CrudConnectorDecorator {
        protected AttachmentHandler _attachmentHandler;

        public BasePurchaseRequestCrudConnector() {
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


            var crudData = ((CrudOperationData)maximoTemplateData.OperationData);
            LongDescriptionHandler.HandleLongDescription(sr, crudData);
            var attachments = crudData.GetRelationship("attachment");
            /*foreach (var attachment in (IEnumerable<CrudOperationData>)attachments)
            {
                HandleAttachmentAndScreenshot(attachment, sr, maximoTemplateData.ApplicationMetadata);
            }*/
            HandlePRLINES(maximoTemplateData, crudData, sr);
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
            _attachmentHandler.HandleAttachmentAndScreenshot(maximoTemplateData);

            HandlePRLINES(maximoTemplateData, crudData, sr);
            base.BeforeCreation(maximoTemplateData);
        }

        private void HandlePRLINES(MaximoOperationExecutionContext maximoTemplateData, CrudOperationData crudDataEntity, object sr) {
            var prlines = (IEnumerable<CrudOperationData>)crudDataEntity.GetRelationship("prline");

            var user = SecurityFacade.CurrentUser();
            w.CloneArray((IEnumerable<CrudOperationData>)crudDataEntity.GetRelationship("prline"), sr, "PRLINE",
                delegate(object integrationObject, CrudOperationData crudData) {
                    if (ReflectionUtil.IsNull(integrationObject, "PRLINENUM")) {
                        //Need to generate a unique PR Line number for each PR
                        //Web sevice doesn't do it and hence do a native query to get the 
                        // maximum value and increment it by 1. 
                        BaseHibernateDAO _dao = MaximoHibernateDAO.GetInstance();
                        String queryst = "Select MAX(prline.prlinenum) from prline where prnum like ";
                        var prnum = w.GetRealValue(sr, "PRNUM");
                        queryst = queryst + Convert.ToString(prnum);
                        var id = _dao.FindSingleByNativeQuery<object>(queryst, null);
                        int prlinenum = Convert.ToInt32(id) + 1;
                        w.SetValue(integrationObject, "PRLINENUM", prlinenum);
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
                    w.SetValueIfNull(integrationObject, "SITEID", user.SiteId);
                    w.SetValueIfNull(integrationObject, "ORGID", user.OrgId);
                    w.SetValueIfNull(integrationObject, "ENTEREDASTASK", false);
                    w.SetValueIfNull(integrationObject, "CONVERTTOCONTRACT", false);
                    w.SetValueIfNull(integrationObject, "LANGCODE", "EN");
                    w.SetValueIfNull(integrationObject, "CONVERSION", 1);
                    w.SetValueIfNull(integrationObject, "HASID", false);
                    w.SetValueIfNull(integrationObject, "PRLINENUM", 0);
                    w.SetValueIfNull(integrationObject, "MKTPLCITEM", false);
                    w.SetValueIfNull(integrationObject, "TAXEXEMPT", false);
                    w.SetValueIfNull(integrationObject, "CONSIGNMENT", false);
                    w.SetValueIfNull(integrationObject, "ITEMNUM", "test");

                    ReflectionUtil.SetProperty(integrationObject, "action", OperationType.Add.ToString());
                });
        }

    }
}
