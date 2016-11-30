using log4net;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Base;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace softWrench.sW4.Data.Persistence.WS.Ism.Entities.Change {
    class IsmChangeCrudConnector : BaseISMDecorator {

        protected static readonly ILog Log = LogManager.GetLogger(typeof(IsmChangeCrudConnector));
        private readonly SWDBHibernateDAO _dao;

        public IsmChangeCrudConnector() {
            _dao = SimpleInjectorGenericFactory.Instance.GetObject<SWDBHibernateDAO>(typeof(SWDBHibernateDAO));
        }

        public override void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            base.BeforeUpdate(maximoTemplateData);
            var entity = (CrudOperationData)maximoTemplateData.OperationData;
            var maximoTicket = (ChangeRequest)maximoTemplateData.IntegrationObject;
            var operationData = (CrudOperationData)maximoTemplateData.OperationData;

            HapagChangeHandler.FillDefaultValuesUpadteChange(maximoTicket);
            maximoTicket.RequesterID = (string)entity.GetAttribute("wonum");
            maximoTicket.ProviderID = string.Empty;

            var description = (String)operationData.GetAttribute("description");
            Log.Debug("Current description of change request is: " + description);

            maximoTicket.Change.Description = description;

            maximoTicket.Change.CustomerID = entity.GetAttribute("pluspcustomer") as string;


            HandleWorkLog(entity, maximoTicket);
            HandleDescription(operationData,description, maximoTicket);
            Log.Debug("Now the description will be: " + maximoTicket.Change.Description);

            var integrationObject = maximoTemplateData.IntegrationObject;
            ISMAttachmentHandler.HandleAttachmentsForUpdate((CrudOperationData)maximoTemplateData.OperationData, (ChangeRequest)integrationObject);
        }

        protected virtual void HandleDescription(CrudOperationData operationData, string description, ChangeRequest maximoTicket) {
            if (description != null && !description.StartsWith("@@")) {
                if (description.Length > 98) {
                    //we need to make sure the size is never bigger than 100
                    //https://controltechnologysolutions.atlassian.net/browse/HAP-642
                    description = "@@" + description.Substring(0, 98);
                } else {
                    description = "@@" + description;
                }
                maximoTicket.Change.Description = description;
            }
        }

        private static Boolean HandleWorkLog(CrudOperationData entity, ChangeRequest maximoTicket) {
            var maximoWorklogs = entity.GetRelationship("worklog_");
            var worklogList = new List<ChangeLog>();
            var user = SecurityFacade.CurrentUser();
            foreach (var jsonWorklog in (IEnumerable<CrudOperationData>)maximoWorklogs) {
                var worklogid = jsonWorklog.GetAttribute("worklogid");
                if (worklogid == null) {
                    var changeLog = new ChangeLog {
                        Log = (string)jsonWorklog.GetAttribute("description"),
                        ActionID = "CLIENTNOTE",
                        UserID = ISMConstants.AddEmailIfNeeded(user.MaximoPersonId),
                        LogDateTimeSpecified = true,
                        LogDateTime = DateTime.Now
                    };
                    var longDesc = "";
                    var ld = (CrudOperationData)jsonWorklog.GetRelationship("longdescription");
                    if (ld != null) {
                        longDesc = (string)ld.GetAttribute("ldtext");
                    }
                    changeLog.FlexFields = new[]{
                        new FlexFieldsFlexField { mappedTo = "WLLongDesc", id = "0",Value = longDesc }
                    };

                    worklogList.Add(changeLog);
                }
            }

            maximoTicket.ChangeLog = ArrayUtil.PushRange(maximoTicket.ChangeLog, worklogList);
            return worklogList.Any();
        }
    }
}
