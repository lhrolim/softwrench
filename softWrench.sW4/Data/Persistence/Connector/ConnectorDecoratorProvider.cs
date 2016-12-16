using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softwrench.sw4.api.classes.application;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Metadata.Applications.DataSet;

namespace softWrench.sW4.Data.Persistence.Connector {
    public class ConnectorDecoratorProvider : ApplicationFiltereableProvider<CrudConnectorDecorator> {

        protected override CrudConnectorDecorator LocateDefaultItem(string applicationName, string schemaId, string clientName) {
            return null;
        }
    }
}
