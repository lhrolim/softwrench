using System;
using softWrench.sW4.Data.Persistence.Engine;
using cts.commons.simpleinjector;
using WcfSamples.DynamicProxy;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using w = softWrench.sW4.Data.Persistence.WS.Internal.WsUtil;

namespace softWrench.sW4.Data.Persistence.WS.API {
    public abstract class CrudConnectorDecorator : IMaximoCrudConnector {
        protected BaseMaximoCrudConnector _realCrudConnector;

        protected internal MaximoConnectorEngine ConnectorEngine {
            get {
                return
                    SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(
                        typeof(MaximoConnectorEngine));
            }
        } 

        public BaseMaximoCrudConnector RealCrudConnector {
            get { return _realCrudConnector; }
            set { _realCrudConnector = value; }
        }

        public virtual DynamicObject CreateProxy(EntityMetadata metadata) {
            return RealCrudConnector.CreateProxy(metadata);
        }

        public virtual MaximoOperationExecutionContext CreateExecutionContext(DynamicObject proxy, IOperationData operationData) {
            return RealCrudConnector.CreateExecutionContext(proxy, operationData);
        }

        public virtual void PopulateIntegrationObject(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.PopulateIntegrationObject(maximoTemplateData);
        }

        public virtual object HandleActualDates(object entity) {
            var statusValue = w.GetRealValue(entity, "STATUS");
            if (statusValue == null) {
                return null;
            }

            if (statusValue.Equals("INPROG") && w.GetRealValue(entity, "ACTUALSTART") == null) {
                w.SetValueIfNull(entity, "ACTUALSTART", DateTime.Now.FromServerToRightKind());
            } else if (statusValue.Equals("RESOLVED") && w.GetRealValue(entity, "ACTUALFINISH") == null) {
                w.SetValue(entity, "ACTUALFINISH", DateTime.Now.FromServerToRightKind());
            }

            return statusValue;
        }

        protected virtual void SetSwChangeBy(object entity) {
            var user = SecurityFacade.CurrentUser();
            // TODO: Temp fix for getting change by to update with the userid. 
            // This workaround required trigger in the Maximo DB and custom attribute "SWCHANGEBY" in ticket
            w.SetValue(entity, "SWCHANGEBY", user.Login);
        }

        #region Create
        public virtual void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.BeforeCreation(maximoTemplateData);
        }
        public virtual void DoCreate(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.DoCreate(maximoTemplateData);
        }
        public virtual void AfterCreation(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.AfterCreation(maximoTemplateData);
        }
        #endregion
        
        #region Retrieve
        public virtual void BeforeFindById(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.BeforeFindById(maximoTemplateData);
        }
        public virtual void DoFindById(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.DoFindById(maximoTemplateData);
        }
        public virtual void AfterFindById(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.AfterFindById(maximoTemplateData);
        }
        #endregion

        #region Update
        public virtual void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.BeforeUpdate(maximoTemplateData);
        }        
        public virtual void DoUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.DoUpdate(maximoTemplateData);
        }        
        public virtual void AfterUpdate(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.AfterUpdate(maximoTemplateData);
        }        
        #endregion

        #region Delete
        public void BeforeDeletion(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.BeforeDeletion(maximoTemplateData);
        }
        public void DoDelete(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.DoDelete(maximoTemplateData);
        }
        public void AfterDeletion(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudConnector.AfterDeletion(maximoTemplateData);
        }
        #endregion
    }
}
