using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Util;

namespace softwrench.sw4.tgcs.classes.com.cts.tgcs.configuration {
    public class ToshibaConfigurationRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {
        
        
        public const string ToshibaPersonSyncRefreshrate = "/Tgcs/Sync/Person/RefreshRate";
        public const string ToshibaSyncPersonUId = "/Tgcs/Sync/Person/PersonUId";

        public const string ToshibaSRSyncRefreshrate = "/Tgcs/Sync/SR/RefreshRate";
        public const string ToshibaSyncSrStatusDate = "/Tgcs/Sync/SR/SRStatusDate";
        public const string ToshibaSyncMaximoThreads = "/Tgcs/Sync/Global/MaximoThreads";

        private readonly IConfigurationFacade _configurationFacade;

        public ToshibaConfigurationRegistry(IConfigurationFacade configurationFacade) {
            _configurationFacade = configurationFacade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ApplicationConfiguration.ClientName != "tgcs") return;

            _configurationFacade.Register(ToshibaSyncSrStatusDate, new PropertyDefinition {
                Description = "Start point statusdate after which ISM SR updates would be synced. If null, no sync would be performed",
                StringValue = null,
                PropertyDataType = PropertyDataType.DATE,
            });

            _configurationFacade.Register(ToshibaSyncPersonUId, new PropertyDefinition {
                Description = "Start point personuid after which ISM SR updates would be synced. If null, no sync would be performed",
                StringValue = null,
                PropertyDataType = PropertyDataType.LONG,
            });

            _configurationFacade.Register(ToshibaPersonSyncRefreshrate, new PropertyDefinition {
                Description = "ISM Person Sync Job Default Refresh Rate",
                StringValue = "12",
                PropertyDataType = PropertyDataType.LONG,
            });

            _configurationFacade.Register(ToshibaSyncMaximoThreads, new PropertyDefinition {
                Description = "Maximo number of threads to execute on Maximo side upon the sync operation",
                StringValue = "5",
                PropertyDataType = PropertyDataType.LONG,
            });


            _configurationFacade.Register(ToshibaSRSyncRefreshrate, new PropertyDefinition {
                Description = "ISM SR Sync Job Default Refresh Rate",
                StringValue = "5",
                PropertyDataType = PropertyDataType.LONG,
            });
        }
    }
}
