using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.WS.API;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    public class MaximoApplicationDataSet : BaseApplicationDataSet {

        private IConnectorEngine _connectorEngine;

        private MaximoHibernateDAO _maxDAO;

        protected override IConnectorEngine Engine() {
            if (_connectorEngine == null) {
                _connectorEngine = SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(typeof(MaximoConnectorEngine));
            }
            return _connectorEngine;
        }

        protected MaximoHibernateDAO MaxDAO {
            get {
                if (_maxDAO == null) {
                    _maxDAO =
                        SimpleInjectorGenericFactory.Instance.GetObject<MaximoHibernateDAO>(
                            typeof(MaximoHibernateDAO));
                }
                return _maxDAO;
            }
        }
    }
}
