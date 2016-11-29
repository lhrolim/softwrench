using System;
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
using softwrench.sw4.api.classes.configuration;

namespace softWrench.sW4.Configuration.Services {

    class ConfigurationFacade : IConfigurationFacade, ISWEventListener<ApplicationStartedEvent>, IOrdered {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationFacade));

        private bool _appStarted;
        private readonly IDictionary<string, PropertyDefinition> _toRegister = new ConcurrentDictionary<string, PropertyDefinition>();
        private readonly IDictionary<string, string> _toOverride = new ConcurrentDictionary<string, string>();

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

        public async Task RegisterAsync(string configKey, PropertyDefinitionRegistry definition) {
            await RegisterAsync(configKey, new PropertyDefinition {
                CachedOnClient = definition.CachedOnClient,
                Description = definition.Description,
                DataType = definition.DataType,
                StringValue = definition.DefaultValue
            });
        }

        public void Register(string configKey, PropertyDefinition definition) {
            AsyncHelper.RunSync(() => RegisterAsync(configKey, definition));
        }

        public void Override(string configKey, string newDefaultValue) {
            if (_appStarted) {
                throw new InvalidOperationException("this method shouldn´t be called after the application has started");
            }


            if (_toRegister.ContainsKey(configKey)) {
                Log.DebugFormat("overriding configkey {0} to new value {1}", configKey, newDefaultValue);
                _toRegister[configKey].StringValue = newDefaultValue;
            } else {
                //case the override method has been called on a listener that ran before the oririnal declaration one
                _toOverride.Add(configKey, newDefaultValue);
            }


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
            foreach (var toOverride in _toOverride) {
                var originalDeclaration = definitions.FirstOrDefault(d => d.FullKey.Equals(toOverride.Key));
                if (originalDeclaration != null) {
                    Log.DebugFormat("overriding configkey {0} to new value {1}", originalDeclaration.FullKey, toOverride.Value);
                    originalDeclaration.StringValue = toOverride.Value;
                } else {
                    Log.WarnFormat("definition {0} not found for override, review the implementation", toOverride.Key);
                }
            }

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
                return 100;
            }
        }
    }
}
