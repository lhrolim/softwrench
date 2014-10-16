using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    public class MaximoApplicationDataSet : BaseApplicationDataSet {

        private IConnectorEngine _connectorEngine;

        protected override IConnectorEngine Engine() {
            if (_connectorEngine == null) {
                _connectorEngine = SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(typeof(MaximoConnectorEngine));
            }
            return _connectorEngine;
        }
    }
}
