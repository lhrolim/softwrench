using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Engine;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {

    public class SWDBApplicationDataset : BaseApplicationDataSet {
        protected override IConnectorEngine Engine() {
            return SimpleInjectorGenericFactory.Instance.GetObject<SWDBConnectorEngine>(typeof(SWDBConnectorEngine));
        }

        protected ISWDBHibernateDAO SWDAO {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<ISWDBHibernateDAO>(typeof(ISWDBHibernateDAO));
            }
        }
    }
}
