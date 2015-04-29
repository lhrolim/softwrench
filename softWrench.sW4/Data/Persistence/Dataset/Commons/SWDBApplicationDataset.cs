using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Engine;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class SWDBApplicationDataset : BaseApplicationDataSet {

        private IConnectorEngine _connectorEngine;

        private ISWDBHibernateDAO _dao;

        protected override IConnectorEngine Engine() {
            if (_connectorEngine == null) {
                _connectorEngine = SimpleInjectorGenericFactory.Instance.GetObject<SWDBConnectorEngine>(typeof(SWDBConnectorEngine));
            }
            return _connectorEngine;
        }

        protected ISWDBHibernateDAO SWDAO {
            get {
                if (_dao == null) {
                    _dao =
                        SimpleInjectorGenericFactory.Instance.GetObject<ISWDBHibernateDAO>(
                            typeof(ISWDBHibernateDAO));
                }
                return _dao;
            }
        }
      
    }
}
