using System;
using cts.commons.simpleinjector.Events;
using log4net;
using softwrench.sw4.api.classes.configuration;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Core {

    public class BaseRedisManager : IBaseRedisManager, ISWEventListener<ConfigurationChangedEvent> {

        private readonly ILog _log = LogManager.GetLogger(typeof(DatamapRedisManager));

        //internal for mocking on tests
        internal ICacheClient CacheClient;

        public bool ServiceAvailable { get; set; }

        private readonly IConfigurationFacade _configFacade;
        //not using Newtonsoft due to conflicts in version 5.0.8 --> 10.0.2
        //many breaking changes, for instance (Configuration Screen)

        private readonly ISerializer _serializer = new CustomJSONSerializer();

        public ConnectionMultiplexer Multiplexer {
            get; set;
        }

        public BaseRedisManager(IConfigurationFacade configFacade) {
            _configFacade = configFacade;


            if (configFacade == null) {
                //to easen the burden for unit tests
                return;
            }


            DoInit();
        }

        private void DoInit() {
            try {
                var redisUrl = _configFacade.Lookup<string>(ConfigurationConstants.Cache.RedisURL);
                if (redisUrl != null) {
                    Multiplexer = ConnectionMultiplexer.Connect(redisUrl);
                    CacheClient = new StackExchangeRedisCacheClient(Multiplexer, _serializer);
                    ServiceAvailable = true;
                }
            } catch (Exception) {
                ServiceAvailable = false;
                CacheClient = null;
                _log.WarnFormat("Redis is not available, or not properly configured. Cache will be ignored");
            }
        }


        public virtual bool IsAvailable() {
            return ServiceAvailable;
        }

        public void HandleEvent(ConfigurationChangedEvent eventDispatched) {
            if (eventDispatched.ConfigKey.Equals(ConfigurationConstants.Cache.RedisURL)) {
                DoInit();
            }
        }
    }
}
