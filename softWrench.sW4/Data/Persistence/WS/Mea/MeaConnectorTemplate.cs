using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Data.Persistence.WS.Internal;

namespace softWrench.sW4.Data.Persistence.WS.Mea
{
    class MeaCrudConnector : BaseMaximoCrudConnector
    {
        //
        public override void DoCreate(MaximoOperationExecutionContext maximoTemplateData) {
            var resultData = maximoTemplateData.InvokeProxy();
            var id = string.Empty;
            maximoTemplateData.ResultObject = new TargetResult(id, id, resultData);
        }

        public override MaximoOperationExecutionContext CreateExecutionContext(WcfSamples.DynamicProxy.DynamicObject proxy, IOperationData operationData) {
            return new MeaExecutionContext(operationData, proxy);
        }
    }
}
