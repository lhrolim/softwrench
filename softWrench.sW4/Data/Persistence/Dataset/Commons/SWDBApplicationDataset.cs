using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class SWDBApplicationDataset : BaseApplicationDataSet {

        private IConnectorEngine _connectorEngine;

        protected override IConnectorEngine Engine() {
            if (_connectorEngine == null) {
                _connectorEngine = SimpleInjectorGenericFactory.Instance.GetObject<SWDBConnectorEngine>(typeof(SWDBConnectorEngine));
            }
            return _connectorEngine;
        }

      
    }
}
