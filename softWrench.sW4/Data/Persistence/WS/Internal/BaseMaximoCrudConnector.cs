﻿using log4net;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Security.Services;
using System;
using WcfSamples.DynamicProxy;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;


namespace softWrench.sW4.Data.Persistence.WS.Internal {
    public abstract class BaseMaximoCrudConnector : IMaximoCrudConnector {

        private static readonly ILog Log = LogManager.GetLogger(typeof(BaseMaximoCrudConnector));

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

            foreach (var attribute in operationData.Attributes) {
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
            var idFieldName = entityMetadata.IdFieldName;
            if (operationData.Id != null) {
                w.SetValueIfNull(integrationObject, idFieldName, operationData.Id);
            }
            if (operationData.UserId != null) {
                w.SetValueIfNull(integrationObject, entityMetadata.UserIdFieldName, operationData.UserId);
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
            var resultOb = (Array)resultData;
            var firstOb = resultOb.GetValue(0);
            var id = WsUtil.GetRealValue(firstOb, idProperty) as string;
            var userId = WsUtil.GetRealValue(firstOb, userIdProperty);
            string siteId = null;
            if (siteIdAttribute != null) {
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
            maximoTemplateData.ResultObject = new TargetResult(id, userId.ToString(), resultData, null, siteId);

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
