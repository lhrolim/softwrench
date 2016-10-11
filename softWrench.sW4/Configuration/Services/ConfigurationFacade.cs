using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Configuration.Util;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using cts.commons.Util;

namespace softWrench.sW4.Configuration.Services {

    class ConfigurationFacade : IConfigurationFacade, ISWEventListener<ApplicationStartedEvent>, IPriorityOrdered {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationFacade));

        private bool _appStarted;
        private readonly IDictionary<string, PropertyDefinition> _toRegister = new ConcurrentDictionary<string, PropertyDefinition>();

        private readonly SWDBHibernateDAO _dao;
        private readonly ConfigurationService _configService;
        private readonly IContextLookuper _contextLookuper;

        public ConfigurationFacade(SWDBHibernateDAO dao, ConfigurationService configService, IContextLookuper contextLookuper) {
            _dao = dao;
            _configService = configService;
            _contextLookuper = contextLookuper;
        }

        public T Lookup<T>(string configKey) {
            return AsyncHelper.RunSync(() => LookupAsync<T>(configKey));
        }

        public async Task<T> LookupAsync<T>(string configKey) {
            var lookupContext = _contextLookuper.LookupContext();
            return await _configService.Lookup<T>(configKey, lookupContext);
        }

        public void Register(string configKey, PropertyDefinition definition) {
            AsyncHelper.RunSync(() => RegisterAsync(configKey, definition));
        }

        public async Task RegisterAsync(string configKey, PropertyDefinition definition) {
            if (!_appStarted) {
                _toRegister.Add(configKey, definition);
            } else {
                await DoRegister(configKey, definition);
            }
        }

        public async Task SetValue(string configkey, object value) {
            if (!_appStarted) {
                //TODO: Handle complex integration tests scenarios here, where the value is modified before application is up ==> config is not up yet
            }
            //var previousValue = Lookup<object>(configkey);
            await _configService.SetValue(configkey, value);
            //_eventDispatcher.Fire(eventToDispatch: new ConfigurationChangedEvent(configkey, previousValue, value), parallel: true);
        }


        private async Task DoRegister(string configKey, PropertyDefinition definition) {
            SetKeys(configKey, definition);
            await _dao.SaveAsync(definition);
        }

        private PropertyDefinition SetKeys(string configKey, PropertyDefinition definition) {
            definition.FullKey = configKey;
            definition.SimpleKey = CategoryUtil.GetPropertyKey(configKey);
            return definition;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var definitions = _toRegister.Select(entry => SetKeys(entry.Key, entry.Value)).ToList();
            _dao.BulkSave(definitions);
            _appStarted = true;
        }

        public async Task<ClientSideConfigurations> GetClientSideConfigurations(long? cacheTimestamp) {
            return await _configService.GetClientSideConfigurations(cacheTimestamp, _contextLookuper.LookupContext());
        }

        public void ConditionAltered(string configKey) {
            _configService.ClearCache(configKey);
        }

        //execute last
        public int Order {
            get {
                return 1;
            }
        }
    }
}
