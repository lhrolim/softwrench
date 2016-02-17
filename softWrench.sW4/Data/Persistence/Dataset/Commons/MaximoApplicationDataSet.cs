using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Engine;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    public class MaximoApplicationDataSet : BaseApplicationDataSet {

        private IConnectorEngine _connectorEngine;

        private IMaximoHibernateDAO _maxDAO;

        protected override IConnectorEngine Engine() {
            if (_connectorEngine == null) {
                _connectorEngine = SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(typeof(MaximoConnectorEngine));
            }
            return _connectorEngine;
        }

        protected IMaximoHibernateDAO MaxDAO {
            get {
                if (_maxDAO == null) {
                    _maxDAO =
                        SimpleInjectorGenericFactory.Instance.GetObject<IMaximoHibernateDAO>(
                            typeof(IMaximoHibernateDAO));
                }
                return _maxDAO;
            }
        }
    }
}
