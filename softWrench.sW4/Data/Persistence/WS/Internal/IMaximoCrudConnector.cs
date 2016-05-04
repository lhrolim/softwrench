using cts.commons.simpleinjector;
using WcfSamples.DynamicProxy;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata.Entities;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    internal interface IMaximoCrudConnector : IMaximoConnector {
        DynamicObject CreateProxy(EntityMetadata metadata);
        MaximoOperationExecutionContext CreateExecutionContext(DynamicObject proxy, IOperationData operationData);
        void PopulateIntegrationObject(MaximoOperationExecutionContext maximoTemplateData);

        #region Create
        void BeforeCreation(MaximoOperationExecutionContext maximoTemplateData);
        void DoCreate(MaximoOperationExecutionContext maximoTemplateData);
        void AfterCreation(MaximoOperationExecutionContext maximoTemplateData);
        #endregion

        #region Retrieve
        void BeforeFindById(MaximoOperationExecutionContext maximoTemplateData);
        void DoFindById(MaximoOperationExecutionContext maximoTemplateData);
        void AfterFindById(MaximoOperationExecutionContext maximoTemplateData);
        #endregion

        #region Update
        void BeforeUpdate(MaximoOperationExecutionContext maximoTemplateData);
        void DoUpdate(MaximoOperationExecutionContext maximoTemplateData);
        void AfterUpdate(MaximoOperationExecutionContext maximoTemplateData);
        #endregion

        #region Delete
        void BeforeDeletion(MaximoOperationExecutionContext maximoTemplateData);
        void DoDelete(MaximoOperationExecutionContext maximoTemplateData);
        void AfterDeletion(MaximoOperationExecutionContext maximoTemplateData);
        #endregion        
    }
}