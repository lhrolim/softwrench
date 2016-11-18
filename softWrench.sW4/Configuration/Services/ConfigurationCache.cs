using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using NHibernate.Util;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Security.Context;

namespace softWrench.sW4.Configuration.Services {
    public class ConfigurationCache : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<ContextHolder, string>> ConfigCache = new ConcurrentDictionary<string, ConcurrentDictionary<ContextHolder, string>>();

        // client cache
        private static ConcurrentBag<string> _cachedOnClientKeyCache = null; // keys cache - once app and this is populated for the first time it never changes.
        private static ConcurrentDictionary<string, string> _cachedOnClientCache = null;
        private static long _cachedOnClientTimestamp = 0;

        private readonly ISWDBHibernateDAO _dao;
        private bool _appStarted;

        public ConfigurationCache(ISWDBHibernateDAO dao) {
            _dao = dao;
        }

        public KeyValuePair<bool, string> GetFromCache(string key, ContextHolder lookupContext) {
            ConcurrentDictionary<ContextHolder, string> fromContext;
            if (!ConfigCache.TryGetValue(key, out fromContext) || !fromContext.ContainsKey(lookupContext)) {
                return new KeyValuePair<bool, string>(false, null);
            }
            return new KeyValuePair<bool, string>(true, fromContext[lookupContext]);
        }

        public void AddToCache(string key, ContextHolder lookupContext, string value) {
            ConcurrentDictionary<ContextHolder, string> fromContext;
            var addFromContext = false;
            if (!ConfigCache.TryGetValue(key, out fromContext)) {
                fromContext = new ConcurrentDictionary<ContextHolder, string>();
                addFromContext = true;
            }
            fromContext.TryAdd(lookupContext, value);
            if (addFromContext) {
                ConfigCache.TryAdd(key, fromContext);
            }
            ClearCachedOnClientIfNeeded(key);
        }

        public virtual void ClearCache(string key) {
            ConcurrentDictionary<ContextHolder, string> fromContext;
            if (ConfigCache.TryGetValue(key, out fromContext)) {
                //clear entire cache of definion
                fromContext.Clear();
            }
            ClearCachedOnClientIfNeeded(key);
        }

        public IEnumerable<string> GetCacheableOnClientKeyCache() {
            if (_cachedOnClientKeyCache == null) {
                BuildCachedOnClientKeyCache();
            }
            return _cachedOnClientKeyCache;
        }

        public ClientSideConfigurations GetClientSideConfigurations() {
            if (_cachedOnClientCache == null) {
                return null;
            }

            return new ClientSideConfigurations() {
                Configurations = _cachedOnClientCache,
                CacheTimestamp = _cachedOnClientTimestamp
            };
        }

        public ClientSideConfigurations UpdateCachedOnClient(IDictionary<string, string> configs, long timestamp) {
            _cachedOnClientTimestamp = timestamp;
            var newConfigs = new ConcurrentDictionary<string, string>(configs);
            _cachedOnClientCache = newConfigs;
            return new ClientSideConfigurations() {
                Configurations = newConfigs,
                CacheTimestamp = timestamp
            };
        }

        private void BuildCachedOnClientKeyCache() {
            var cachedOnClientKeyCache = new ConcurrentBag<string>();
            var definitions = _dao.FindByQuery<PropertyDefinition>(PropertyDefinition.ByCachedOnClient, true);
            if (definitions == null || !definitions.Any()) {
                _cachedOnClientKeyCache = cachedOnClientKeyCache;
                return;
            }
            definitions.ForEach(def => cachedOnClientKeyCache.Add(def.FullKey));
            _cachedOnClientKeyCache = cachedOnClientKeyCache;
        }

        private void ClearCachedOnClientIfNeeded(string key) {
            if (_dao == null || !_appStarted) {
                return;
            }
            var keyCache = GetCacheableOnClientKeyCache();
            if (keyCache.Contains(key)) {
                _cachedOnClientCache = null;
            }
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            _appStarted = true;
        }
    }
}
