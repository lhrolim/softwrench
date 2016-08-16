using log4net;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Security.Services;
using System;
using Microsoft.Ajax.Utilities;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using WcfSamples.DynamicProxy;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;


namespace softWrench.sW4.Data.Persistence.WS.Internal {
    public abstract class BaseMaximoCrudConnector : IMaximoCrudConnector {

        protected static readonly ILog Log = LogManager.GetLogger(typeof(BaseMaximoCrudConnector));

        public virtual DynamicObject CreateProxy(EntityMetadata metadata) {
            return DynamicProxyUtil.LookupProxy(metadata);
        }

        public abstract MaximoOperationExecutionContext CreateExecutionContext(
            DynamicObject proxy, IOperationData operationData);

        public virtual void PopulateIntegrationObject(MaximoOperationExecutionContext maximoExecutionContext) {

            object integrationObject = maximoExecutionContext.IntegrationObject;
            var operationData = (CrudOperationData)maximoExecutionContext.OperationData;
            var entityMetadata = maximoExecutionContext.OperationData.EntityMetadata;
            w.SetValueIfNull(integrationObject, "class", operationData.Class);
            TargetConstantHandler.SetConstantValues(integrationObject, entityMetadata);
            TargetAttributesHandler.SetValuesFromJSON(integrationObject, entityMetadata, operationData);


            long id;
            if (long.TryParse(operationData.Id, out id) && id < 0) {
                // The negative ID is used by the front end to identify new records. 
                //The back end does not support this. therefore, it should be nullified when submitted to Maximo
                operationData.Id = null;
                operationData[entityMetadata.IdFieldName] = null;
            }


            foreach (var attribute in operationData) {
                if (attribute.Value == null) {
                    continue;
                }
                try {
                    w.SetValue(integrationObject, attribute.Key.ToUpper(), attribute.Value);
                } catch (Exception e) {
                    var entityName = maximoExecutionContext.Metadata.Name;
                    throw new InvalidOperationException(String.Format("Error setting property {0} of entity {1}. {2}",
                                                                      attribute.Key.ToUpper(), entityName, e.Message), e);
                }
            }
            var pluspCustomer = MetadataProvider.GlobalProperty(SwConstants.MultiTenantPrefix);
            if (pluspCustomer != null) {
                w.SetValue(integrationObject, "PLUSPCUSTOMER", pluspCustomer);
            }

            var idFieldName = entityMetadata.IdFieldName;
            if (!string.IsNullOrEmpty(operationData.Id)) {
                w.SetValueIfNull(integrationObject, idFieldName, operationData.Id);
            }
            if (!string.IsNullOrEmpty(operationData.UserId)) {
                w.SetValueIfNull(integrationObject, entityMetadata.UserIdFieldName, operationData.UserId);
            }
            if (!string.IsNullOrEmpty(operationData.SiteId)) {
                w.SetValueIfNull(integrationObject, "siteid", operationData.SiteId);
            }

            var curUser = SecurityFacade.CurrentUser();
            w.SetValueIfNull(integrationObject, "ORGID", curUser.OrgId);
            w.SetValueIfNull(integrationObject, "SITEID", curUser.SiteId);

            // Set uppercase for maximo users (usernames are upper case and case sensitive)            
            SetUppercase(integrationObject, operationData, "affectedperson");
            SetUppercase(integrationObject, operationData, "reportedby");
        }

        #region Create
        public void BeforeCreation(MaximoOperationExecutionContext maximoExecutionContext) {
            //NOOP
        }
        public virtual void DoCreate(MaximoOperationExecutionContext maximoTemplateData) {
            var resultData = maximoTemplateData.InvokeProxy();
            var idProperty = maximoTemplateData.Metadata.Schema.IdAttribute.Name;
            var siteIdAttribute = maximoTemplateData.Metadata.Schema.SiteIdAttribute;
            var userIdProperty = maximoTemplateData.Metadata.Schema.UserIdAttribute.Name;
            var resultOb = resultData == null ? null : (Array)resultData;
            var firstOb = resultOb == null ? null : resultOb.GetValue(0);
            var id = firstOb == null ? null : WsUtil.GetRealValue(firstOb, idProperty);
            var userId = firstOb == null ? null : WsUtil.GetRealValue(firstOb, userIdProperty);
            string siteId = null;
            if (siteIdAttribute != null && firstOb != null) {
                //not all entities will have a siteid...
                siteId = WsUtil.GetRealValue(firstOb, siteIdAttribute.Name) as string;
            }
            if (!idProperty.Equals(userIdProperty) && userId == null) {
                Log.WarnFormat("User Identifier {0} not received after creating object in Maximo.", idProperty);
                maximoTemplateData.ResultObject = new TargetResult(null, null, resultData, null, siteId);
                return;
            }
            if (id == null && userId == null) {
                Log.WarnFormat("Identifier {0} not received after creating object in Maximo.", idProperty);
                maximoTemplateData.ResultObject = new TargetResult(null, null, resultData, null, siteId);
                return;
            }
            if (id == null) {
                Log.WarnFormat("Identifier {0} not received after creating object in Maximo.", idProperty);
                maximoTemplateData.ResultObject = new TargetResult(null, userId.ToString(), resultData, null, siteId);
                return;
            }
            maximoTemplateData.ResultObject = new TargetResult(id.ToString(), userId.ToString(), resultData, null, siteId);

        }
        public void AfterCreation(MaximoOperationExecutionContext maximoExecutionContext) {
            //NOOP
        }
        #endregion

        #region Retrieve
        public void BeforeFindById(MaximoOperationExecutionContext maximoExecutionContext) {
            //NOOP
        }
        public virtual void DoFindById(MaximoOperationExecutionContext maximoTemplateData) {
            var resultData = maximoTemplateData.FindById(maximoTemplateData.OperationData.Id);
            maximoTemplateData.ResultObject = new TargetResult(maximoTemplateData.OperationData.Id, maximoTemplateData.OperationData.UserId, resultData);
        }
        public void AfterFindById(MaximoOperationExecutionContext maximoExecutionContext) {
            //NOOP
        }
        #endregion

        #region Update
        public void BeforeUpdate(MaximoOperationExecutionContext maximoExecutionContext) {
            //NOOP
        }
        public virtual void DoUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            var resultData = maximoTemplateData.InvokeProxy();
            maximoTemplateData.ResultObject = new TargetResult(maximoTemplateData.OperationData.Id, maximoTemplateData.OperationData.UserId, resultData);
        }
        public void AfterUpdate(MaximoOperationExecutionContext maximoExecutionContext) {
            //NOOP
        }
        #endregion

        #region Delete
        public void BeforeDeletion(MaximoOperationExecutionContext maximoTemplateData) {
            //NOOP
        }
        public void DoDelete(MaximoOperationExecutionContext maximoTemplateData) {
            var resultData = maximoTemplateData.InvokeProxy();
            maximoTemplateData.ResultObject = new TargetResult(maximoTemplateData.OperationData.Id, maximoTemplateData.OperationData.UserId, resultData);
        }
        public void AfterDeletion(MaximoOperationExecutionContext maximoTemplateData) {
            //NOOP
        }
        #endregion

        private void SetUppercase(object integrationObject, CrudOperationData operationData, String attribute) {
            var value = operationData.GetAttribute(attribute);
            if (value != null) {
                w.SetValue(integrationObject, attribute.ToUpper(), value.ToString().ToUpper());
            }
        }
    }
}
