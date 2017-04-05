﻿using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;

namespace softwrench.sw4.offlineserver.services.util {
    public class OffLineConfigurationRegister : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private readonly IConfigurationFacade _configFacade;

        public OffLineConfigurationRegister(IConfigurationFacade configFacade) {
            _configFacade = configFacade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            _configFacade.Register(OfflineConstants.AsyncBatchMinSize, new PropertyDefinition {
                Description = "minimum size of the batch so that it executes asynchronously",
                StringValue = "2",
                PropertyDataType = PropertyDataType.LONG,
            });

            _configFacade.Register(OfflineConstants.MaxDownloadSize, new PropertyDefinition {
                Description = "maximum number of entries to download at a single operation",
                StringValue = "50000",
                DefaultValue = "50000",
                PropertyDataType = PropertyDataType.LONG,
            });


            _configFacade.Register(OfflineConstants.MaxAssociationThreads, new PropertyDefinition {
                Description = "maximum level of parallelism to fetch associations on a sync operation",
                StringValue = "4",
                DefaultValue = "4",
                PropertyDataType = PropertyDataType.INT
            });

            _configFacade.Register(OfflineConstants.SupportContactEmail, new PropertyDefinition() {
                Description = "Support email the offline app should contact",
                PropertyDataType = PropertyDataType.STRING,
                StringValue = "lrolim@controltechnologysolutions.com",
                DefaultValue = "lrolim@controltechnologysolutions.com"
            });
        }
    }
}
