using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.configuration {
    public class BatchConfigurationRegistry : ISWEventListener<ApplicationStartedEvent> {

        public const string BatchMaximoThreads = "/Global/Batches/MaximoThreads";

        private readonly IConfigurationFacade _configurationFacade;

        public BatchConfigurationRegistry(IConfigurationFacade configurationFacade) {
            _configurationFacade = configurationFacade;
        }


        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            _configurationFacade.Register(BatchMaximoThreads, new PropertyDefinition {
                Description = "Maximo number of threads to execute on Maximo side upon the sync operation",
                StringValue = "5",
                DataType = "long",
            });
        }
    }
}
