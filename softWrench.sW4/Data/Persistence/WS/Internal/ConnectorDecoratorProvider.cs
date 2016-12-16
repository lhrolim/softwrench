using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.application;

namespace softWrench.sW4.Data.Persistence.WS.Internal {
    public class ConnectorDecoratorProvider : ApplicationFiltereableProvider<IConnectorDecorator> {

        public static ConnectorDecoratorProvider GetInstance() {
            return SimpleInjectorGenericFactory.Instance.GetObject<ConnectorDecoratorProvider>(typeof(ConnectorDecoratorProvider));
        }

        protected override IConnectorDecorator LocateDefaultItem(string applicationName, string schemaId, string clientName) {
            return null;
        }
    }
}
