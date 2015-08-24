using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;

namespace softWrench.sW4.Data.Persistence.WS.Mif {
    internal class MifCrudConnector : BaseMaximoCrudConnector {
        //        private log4net.ILog LOG = LogManager.GetLogger(typeof(MifCrudConnector));

        public override MaximoOperationExecutionContext CreateExecutionContext(WcfSamples.DynamicProxy.DynamicObject proxy, IOperationData operationData) {
            return new MifExecutionContext(operationData, proxy);
        }

    }
}
