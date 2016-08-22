using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.configuration {
    public class ToshibaConfigurationRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {
        public const string ToshibaSyncSrStatusDate = "/Tgcs/Sync/SRStatusDate";
        public const string ToshibaSyncRefreshrate = "/Tgcs/Sync/RefreshRate";
        public const string ToshibaSyncMaximoThreads = "/Tgcs/Sync/MaximoThreads";

        private readonly IConfigurationFacade _configurationFacade;

        public ToshibaConfigurationRegistry(IConfigurationFacade configurationFacade) {
            _configurationFacade = configurationFacade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            _configurationFacade.Register(ToshibaSyncSrStatusDate, new PropertyDefinition {
                Description = "Start point statusdate after which ISM SR updates would be synced. If null, no sync will be performed",
                StringValue = null,
                DataType = "date",
            });

            _configurationFacade.Register(ToshibaSyncMaximoThreads, new PropertyDefinition {
                Description = "Maximo number of threads to execute on Maximo side upon the sync operation",
                StringValue = "5",
                DataType = "long",
            });


            _configurationFacade.Register(ToshibaSyncRefreshrate, new PropertyDefinition {
                Description = "ISM SR Sync Job Default Refresh Rate",
                StringValue = "5",
                DataType = "long",
            });
        }
    }
}
