using cts.commons.simpleinjector;
using softwrench.sw4.api.classes.application;

namespace softWrench.sW4.Data.Persistence.WS.Internal
{
    public interface IConnectorDecorator : IMaximoConnector, IActionpplicationFiltereable, IComponent {
         
    }
}