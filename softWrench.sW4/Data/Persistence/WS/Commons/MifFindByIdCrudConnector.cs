using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Mif;
using softWrench.sW4.Metadata.Entities;
using WcfSamples.DynamicProxy;

namespace softWrench.sW4.Data.Persistence.WS.Commons {

    /// <summary>
    /// Workaround for getting the Attachments by WS (only operation that uses CrudFindById) from MIF, 
    /// even is targetmapping is different (ISM or MEA)
    /// </summary>
    class MifFindByIdCrudConnector : CrudConnectorDecorator {

        public BaseMaximoCrudConnector RealCrudFindByIdConnector = new MifCrudConnector();

        public override DynamicObject CreateProxy(EntityMetadata metadata) {
            return RealCrudFindByIdConnector.CreateProxy(metadata);
        }

        public override MaximoOperationExecutionContext CreateExecutionContext(DynamicObject proxy, IOperationData operationData) {
            return RealCrudFindByIdConnector.CreateExecutionContext(proxy, operationData);
        }

        public override void BeforeFindById(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudFindByIdConnector.BeforeFindById(maximoTemplateData);
        }
        public override void DoFindById(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudFindByIdConnector.DoFindById(maximoTemplateData);
        }

        public override void AfterFindById(MaximoOperationExecutionContext maximoTemplateData) {
            RealCrudFindByIdConnector.AfterFindById(maximoTemplateData);
        }

        
        public override string ApplicationName() {
            return null;
        }
    }
}
