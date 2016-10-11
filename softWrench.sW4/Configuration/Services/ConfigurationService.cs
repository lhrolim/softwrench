using Iesi.Collections.Generic;
using log4net;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Util;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector.Events;
using JetBrains.Annotations;
using NHibernate.Util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Util;

namespace softWrench.sW4.Configuration.Services {

    public class ConfigurationService : ISingletonComponent {

        private const string NoLookupCondition = "Multiple values found for definition {0}, but no lookupcondition specified. Applying default";
        private const string NoPropertiesForContext = "No Properties found for context {0}, retrieving default value";

        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationService));

        private readonly ISWDBHibernateDAO _dao;
        private readonly ConfigurationCache _cache;
        private readonly IEventDispatcher _eventDispatcher;

        public ConfigurationService(ISWDBHibernateDAO dao, ConfigurationCache cache, IEventDispatcher eventDispatcher) {
            _dao = dao;
            _cache = cache;
            _eventDispatcher = eventDispatcher;
        }

        [CanBeNull]
        public async Task<T> Lookup<T>(string configKey, ContextHolder lookupContext, ConfigurationCacheContext cacheContext = null) {
            var ignoreCache = cacheContext != null && cacheContext.IgnoreCache;
            var preDefinition = cacheContext != null ? cacheContext.PreDefinition : null;

            if (!ignoreCache) {
                var fromCache = _cache.GetFromCache(configKey, lookupContext);
                if (fromCache.Key) {
                    Log.DebugFormat("Retrieving key {0} from cache. Retrieved content: {1}", lookupContext, fromCache.Value);
                    return GetConvertedValue<T>(fromCache.Value);
                }
            }
            var definition = preDefinition ?? await _dao.FindSingleByQueryAsync<PropertyDefinition>(PropertyDefinition.ByKey, configKey);
            if (definition == null) {
                Log.Warn(string.Format("property {0} not found", configKey));
                return default(T);
            }
            var values = definition.Values;
            if (!values.Any()) {
                return DefaultValueCase<T>(definition, lookupContext, ignoreCache);
            }
            if (values.Count == 1) {
                var propertyValue = values.First();
                if (!ConditionMatch.No.Equals(propertyValue.MatchesConditions(lookupContext).MatchType)) {
                    return ValueMatched<T>(propertyValue, definition.FullKey, lookupContext, ignoreCache);
                }
                return DefaultValueCase<T>(definition, lookupContext, ignoreCache);
            }
            //TODO: finish Conditions
            return HandleConditions<T>(values, lookupContext, definition, ignoreCache);
        }

        public async Task SetValue(string configKey, object value) {
            var definition = await _dao.FindSingleByQueryAsync<PropertyDefinition>(PropertyDefinition.ByKey, configKey);
            if (definition == null) {
                throw new InvalidOperationException(string.Format("Property {0} not found", configKey));
            }
            if (definition.Contextualized) {
                throw new InvalidOperationException(string.Format("Cannot modify value of a contextualized definition {0} programatically", configKey));
            }
            var foundValue = definition.Values.FirstOrDefault(d => d.Condition == null);
            if (foundValue == null) {
                foundValue = new PropertyValue { Definition = definition };
            }
            if (foundValue.Definition.DataType.EqualsAny("date", "datetime")) {
                HandleDateTimeValue(configKey, value, foundValue);
            } else {
                foundValue.StringValue = value.ToString();
            }

            await _dao.SaveAsync(foundValue);
            _cache.ClearCache(configKey);
        }

        private static void HandleDateTimeValue(string configKey, object value, PropertyValue foundValue) {
            if (value is DateTime) {
                //making sure to store datetimes under a unified format
                foundValue.StringValue = ((DateTime)value).ToString(DateUtil.DefaultUsaDateTimeFormat);
            } else {
                var convertedDate = DateUtil.Parse(value.ToString());
                if (convertedDate == null) {
                    throw new InvalidOperationException(string.Format("Cannot convert value {0} of key {1} to date type", value,
                        configKey));
                }
                foundValue.StringValue = convertedDate.Value.ToString(DateUtil.DefaultUsaDateTimeFormat);
            }
        }

        private T HandleConditions<T>(IEnumerable<PropertyValue> values, ContextHolder lookupContext, PropertyDefinition definition, bool ignoreCache = false) {
            var resultingValues = BuildResultValues(values, lookupContext);
            if (resultingValues.Any()) {
                return ValueMatched<T>(resultingValues.First().Value, definition.FullKey, lookupContext, ignoreCache);
            }

            if (lookupContext.Module != null) {
                //if a module was asked and nothing was found do not 
                return default(T);
            }

            Log.Debug(string.Format(NoPropertiesForContext, lookupContext));
            return DefaultValueCase<T>(definition, lookupContext, ignoreCache);
        }

        public static SortedDictionary<ConditionMatchResult, PropertyValue> BuildResultValues(IEnumerable<PropertyValue> values, ContextHolder lookupContext) {
            var resultingValues = new SortedDictionary<ConditionMatchResult, PropertyValue>();
            foreach (var propertyValue in values) {
                var conditionMatchResult = propertyValue.MatchesConditions(lookupContext);
                if (!conditionMatchResult.MatchType.Equals(ConditionMatch.No)) {
                    Log.DebugFormat("Property \"{0}\" match for context \"{1}\"", propertyValue, lookupContext);
                    resultingValues.Add(conditionMatchResult, propertyValue);
                }
            }
            return resultingValues;
        }

        private T ValueMatched<T>(PropertyValue propertyValue, string fullKey, ContextHolder configCacheKey, bool ignoreCache = false) {
            var value = propertyValue.StringValue;
            if (string.IsNullOrEmpty(propertyValue.StringValue)) {
                value = propertyValue.SystemStringValue;
            }

            if (!ignoreCache) {
                try {
                    Log.DebugFormat("adding key {0} to configuration cache", configCacheKey);
                    _cache.AddToCache(fullKey, configCacheKey, value);
                } catch (Exception e) {
                    Log.Warn("error updating context cache", e);
                }
            }
            Log.DebugFormat("Value matched: {0}", value);


            return GetConvertedValue<T>(value);
        }

        private T DefaultValueCase<T>(PropertyDefinition definition, ContextHolder configCacheKey, bool ignoreCache = false) {
            if (!ignoreCache) {
                _cache.AddToCache(definition.FullKey, configCacheKey, definition.StringValue);
            }

            return GetConvertedValue<T>(definition.StringValue);
        }


        public async Task<SortedSet<PropertyDefinition>> UpdateDefinitions(CategoryDTO category) {
            var definitions = category.Definitions;
            var updatedDefinitions = new SortedSet<PropertyDefinition>();
            var condition = category.Condition == null ? null : category.Condition.RealCondition;

            //TODO: we are only retrieving again, because a bug where the definition values are not sent over the wire
            var fullKeys = definitions.Select(def => def.FullKey).ToArray();
            var propertyDefinitions = await _dao.FindByQueryAsync<PropertyDefinition>(PropertyDefinition.MultipleByKey, new object[] { fullKeys });
            updatedDefinitions.AddAll(updatedDefinitions);

            foreach (var definition in propertyDefinitions) {
                //var definition = await _dao.FindByPKAsync<PropertyDefinition>(typeof(PropertyDefinition), def.FullKey);
                updatedDefinitions.Add(definition);
                string value;
                if (!category.ValuesToSave.TryGetValue(definition.FullKey, out value)) {
                    //we didn´t touch at these definitions
                    continue;
                }

                var propValue = await _dao.FindSingleByQueryAsync<PropertyValue>(PropertyValue.ByDefinitionConditionModuleProfile, definition.FullKey,
                    condition, category.Module.Id, category.UserProfile);
                if (propValue != null) {
                    await UpdateFoundValue(definition, value, propValue);
                } else if (value != null) {
                    propValue = await _dao.SaveAsync(new PropertyValue {
                        Definition = definition,
                        Module = category.Module.Id,
                        UserProfile = category.UserProfile,
                        Condition = condition,
                        StringValue = value
                    });
                    if (definition.Values == null) {
                        definition.Values = new LinkedHashSet<PropertyValue>();
                    }
                    definition.Values.Add(propValue);
                    _cache.ClearCache(definition.FullKey);
                }

            }

            return updatedDefinitions;
        }

        // global property, ignores module, profile and conditions
        public async Task UpdateGlobalDefinition(string fullKey, string value) {
            var definition = await _dao.FindSingleByQueryAsync<PropertyDefinition>(PropertyDefinition.ByKey, fullKey);
            if ("boolean".Equals(definition.DataType) && value != null) {
                value = value.ToLower(); // newtonsoft json workaround it converts bool true to string "True" same for false
            }
            var propValue = InnerGetGlobalPropertyValue(definition);
            if (propValue != null) {
                await UpdateFoundValue(definition, value, propValue);
                return;
            }

            if (value == null) {
                return;
            }

            propValue = await _dao.SaveAsync(new PropertyValue {
                Definition = definition,
                StringValue = value
            });
            if (definition.Values == null) {
                definition.Values = new LinkedHashSet<PropertyValue>();
            }
            definition.Values.Add(propValue);
            await _dao.SaveAsync(definition);
            _cache.ClearCache(fullKey);

            _eventDispatcher.Fire(eventToDispatch: new ConfigurationChangedEvent(definition.FullKey, null, value), parallel: true);
        }

        // global property, ignores module, profile and conditions
        public T GetGlobalPropertyValue<T>(PropertyDefinition definition) {
            var value = InnerGetGlobalPropertyValue(definition);
            var stringValue = value == null ? null : (value.StringValue ?? value.SystemStringValue);
            return GetConvertedValue<T>(stringValue);
        }

        private static T GetConvertedValue<T>(string stringValue) {
            if (stringValue == null) {
                return default(T);
            }
            var conversionType = typeof(T);
            if (conversionType == typeof(DateTime) || conversionType == typeof(DateTime?)) {
                var dt = DateUtil.Parse(stringValue);
                if (dt == null) {
                    Log.WarnFormat("couldn´t convert date {0}... returning null", stringValue);
                    return default(T);
                }

                return (T)Convert.ChangeType(dt, Nullable.GetUnderlyingType(conversionType) ?? conversionType);
            }

            return (T)Convert.ChangeType(stringValue, Nullable.GetUnderlyingType(conversionType) ?? conversionType);
        }

        // global property, ignores module, profile and conditions
        private static PropertyValue InnerGetGlobalPropertyValue(PropertyDefinition definition) {
            var values = definition.Values;
            return values == null || !values.Any() ? null : values.First();
        }

        private async Task UpdateFoundValue(PropertyDefinition definition, string value, PropertyValue propValue) {
            var previousValue = propValue.Value;
            if (string.IsNullOrEmpty(value) && propValue.SystemValue == null) {
                await _dao.DeleteAsync(propValue);
                if (definition.Values != null && definition.Values.Contains(propValue)) {
                    definition.Values.Remove(propValue);
                }
            } else {
                propValue.StringValue = value;
                propValue = await _dao.SaveAsync(propValue);
                if (definition.Values == null) {
                    definition.Values = new LinkedHashSet<PropertyValue>();
                }
                if (definition.Values.Contains(propValue)) {
                    definition.Values.Remove(propValue);
                }
                definition.Values.Add(propValue);
            }
            _cache.ClearCache(definition.FullKey);

            _eventDispatcher.Fire(eventToDispatch: new ConfigurationChangedEvent(definition.FullKey, previousValue, value), parallel: true);
        }

        public async Task<ClientSideConfigurations> GetClientSideConfigurations(long? cacheTimestamp, ContextHolder lookupContex) {
            var configs = _cache.GetClientSideConfigurations();
            if (configs != null) {
                return cacheTimestamp != null && cacheTimestamp >= configs.CacheTimestamp ? null : configs;
            }

            var keyCache = _cache.GetCacheableOnClientKeyCache();
            var cache = keyCache as IList<string> ?? keyCache.ToList();

            if (!cache.Any()) {
                return _cache.UpdateCachedOnClient(new Dictionary<string, string>(), CurrentTimestamp());
            }

            var cacheableOnClientValues = new Dictionary<string, string>();
            foreach (var key in cache){
                cacheableOnClientValues.Add(key, await Lookup<string>(key, lookupContex));
            }
            return _cache.UpdateCachedOnClient(cacheableOnClientValues, CurrentTimestamp());
        }

        public void ClearCache(string configKey) {
            _cache.ClearCache(configKey);
        }

        private static long CurrentTimestamp() {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }
    }
}
