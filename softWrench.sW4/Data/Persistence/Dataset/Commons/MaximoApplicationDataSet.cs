using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Engine;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Data.Persistence.Dataset.Commons {
    public class MaximoApplicationDataSet : BaseApplicationDataSet {

        protected override IConnectorEngine Engine() {
            return SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(typeof(MaximoConnectorEngine));
        }

        protected IMaximoHibernateDAO MaxDAO {
            get {
                return SimpleInjectorGenericFactory.Instance.GetObject<IMaximoHibernateDAO>(typeof(IMaximoHibernateDAO));
            }
        }
    }
}
