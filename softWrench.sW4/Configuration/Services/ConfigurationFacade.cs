using System;
using System.Collections.Generic;
using log4net;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Configuration.Util;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;

namespace softWrench.sW4.Configuration.Services {

    class ConfigurationFacade : IConfigurationFacade, ISWEventListener<ApplicationStartedEvent>, IPriorityOrdered {


        private Boolean _appStarted;
        private readonly IDictionary<string, PropertyDefinition> _toRegister = new Dictionary<string, PropertyDefinition>();
        private readonly SWDBHibernateDAO _dao;
        private readonly ConfigurationService _configService;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationFacade));

        private readonly IContextLookuper _contextLookuper;


        public ConfigurationFacade(SWDBHibernateDAO dao, ConfigurationService configService, IContextLookuper contextLookuper) {
            _dao = dao;
            _configService = configService;
            _contextLookuper = contextLookuper;
        }

        public T Lookup<T>(string configKey) {
            var lookupContext = _contextLookuper.LookupContext();
            return _configService.Lookup<T>(configKey, lookupContext);
        }

        public void Register(string configKey, PropertyDefinition definition) {
            if (!_appStarted) {
                _toRegister.Add(configKey, definition);
            } else {
                DoRegister(configKey, definition);
            }
        }

        public void SetValue(string configkey, object value) {
            if (!_appStarted) {
                //TODO: Handle complex integration tests scenarios here, where the value is modified before application is up ==> config is not up yet
            }
            _configService.SetValue(configkey, value);
        }


        private void DoRegister(string configKey, PropertyDefinition definition) {
            definition.FullKey = configKey;
            definition.SimpleKey = CategoryUtil.GetPropertyKey(configKey);
            _dao.Save(definition);
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            foreach (var entry in _toRegister) {
                DoRegister(entry.Key, entry.Value);
            }
            _appStarted = true;
        }

        public ClientSideConfigurations GetClientSideConfigurations(long? cacheTimestamp) {
            return _configService.GetClientSideConfigurations(cacheTimestamp, _contextLookuper.LookupContext());
        }

        public void ConditionAltered(string configKey) {
            _configService.ClearCache(configKey);
        }

        //execute last
        public int Order { get { return 1; } }
    }
}
