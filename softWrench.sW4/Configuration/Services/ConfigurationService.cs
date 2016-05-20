﻿using Iesi.Collections.Generic;
using log4net;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Util;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using JetBrains.Annotations;
using softWrench.sW4.Configuration.Services.Api;

namespace softWrench.sW4.Configuration.Services {

    public class ConfigurationService : ISingletonComponent {

        private const string NoLookupCondition = "Multiple values found for definition {0}, but no lookupcondition specified. Applying default";
        private const string NoPropertiesForContext = "No Properties found for context {0}, retrieving default value";

        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationService));

        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<ContextHolder, string>> ConfigCache = new ConcurrentDictionary<string, ConcurrentDictionary<ContextHolder, string>>();

        private readonly ISWDBHibernateDAO _dao;


        public ConfigurationService(ISWDBHibernateDAO dao) {
            _dao = dao;
        }

        [NotNull]
        public T Lookup<T>(string configKey, ContextHolder lookupContext, ConfigurationCacheContext cacheContext = null) {
            ConcurrentDictionary<ContextHolder, string> fromContext = null;
            var ignoreCache = cacheContext != null && cacheContext.IgnoreCache;
            var preDefinition = cacheContext != null ? cacheContext.PreDefinition : null;

            if (!ignoreCache) {
                if (ConfigCache.TryGetValue(configKey, out fromContext)) {
                    if (fromContext.ContainsKey(lookupContext)) {
                        var convertedValue = (T)Convert.ChangeType(fromContext[lookupContext], typeof(T));
                        Log.DebugFormat("Retrieving key {0} from cache. Retrieved content: {1}", lookupContext, convertedValue);
                        return convertedValue;
                    }
                } else {
                    fromContext = new ConcurrentDictionary<ContextHolder, string>();
                    ConfigCache.TryAdd(configKey, fromContext);
                }
            }
            var definition = preDefinition ?? _dao.FindSingleByQuery<PropertyDefinition>(PropertyDefinition.ByKey, configKey);
            if (definition == null) {
                Log.Warn(String.Format("property {0} not found", configKey));
                return default(T);
            }
            var values = definition.Values;
            if (!values.Any()) {
                return DefaultValueCase<T>(definition, fromContext, lookupContext, ignoreCache);
            }
            if (values.Count == 1) {
                var propertyValue = values.First();
                if (!ConditionMatch.No.Equals(propertyValue.MatchesConditions(lookupContext).MatchType)) {
                    return ValueMatched<T>(propertyValue, fromContext, lookupContext, ignoreCache);
                }
                return DefaultValueCase<T>(definition, fromContext, lookupContext, ignoreCache);
            }
            //TODO: finish Conditions
            return HandleConditions<T>(values, lookupContext, definition, fromContext, lookupContext, ignoreCache);
        }

        public void SetValue(string configKey, object value) {
            var definition = _dao.FindSingleByQuery<PropertyDefinition>(PropertyDefinition.ByKey, configKey);
            if (definition == null) {
                throw new InvalidOperationException(String.Format("Property {0} not found", configKey));
            }
            if (definition.Contextualized) {
                throw new InvalidOperationException(String.Format("Cannot modify value of a contextualized definition {0} programatically", configKey));
            }
            var foundValue = definition.Values.FirstOrDefault(d => d.Condition == null);
            if (foundValue == null) {
                foundValue = new PropertyValue();
                foundValue.Definition = definition;
            }
            foundValue.StringValue = value.ToString();
            _dao.Save(foundValue);
            UpdateCache(configKey);
        }



        private void UpdateCache(string configKey) {
            if (ConfigCache.ContainsKey(configKey)) {
                ConcurrentDictionary<ContextHolder, string> value;
                ConfigCache.TryRemove(configKey, out value);
            }
        }

        private T HandleConditions<T>(IEnumerable<PropertyValue> values, ContextHolder lookupContext,
            PropertyDefinition definition, ConcurrentDictionary<ContextHolder, string> fromContext, ContextHolder configCacheKey, bool ignoreCache = false) {
            var resultingValues = BuildResultValues<T>(values, lookupContext);
            if (!resultingValues.Any()) {
                if (lookupContext.Module != null) {
                    //if a module was asked and nothing was found do not 
                    return default(T);
                }

                Log.Debug(String.Format(NoPropertiesForContext, lookupContext));
                return DefaultValueCase<T>(definition, fromContext, configCacheKey, ignoreCache);
            }

            return ValueMatched<T>(resultingValues.First().Value, fromContext, configCacheKey, ignoreCache);

        }

        public static SortedDictionary<ConditionMatchResult, PropertyValue> BuildResultValues<T>(IEnumerable<PropertyValue> values, ContextHolder lookupContext) {
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

        private static T ValueMatched<T>(PropertyValue propertyValue, IDictionary<ContextHolder, string> fromContext, ContextHolder configCacheKey, bool ignoreCache = false) {
            var value = propertyValue.StringValue;
            if (string.IsNullOrEmpty(propertyValue.StringValue)) {
                value = propertyValue.SystemStringValue;
            }

            if (!ignoreCache) {
                try {
                    Log.DebugFormat("adding key {0} to configuration cache", configCacheKey);
                    fromContext.Add(configCacheKey, value);
                } catch (Exception e) {
                    Log.Warn("error updating context cache", e);
                }
            }

            var convertedValue = (T)Convert.ChangeType(value, typeof(T));

            Log.DebugFormat("Value matched: {0}", value);
            return convertedValue;
        }

        private T DefaultValueCase<T>(PropertyDefinition definition, ConcurrentDictionary<ContextHolder, string> fromContext, ContextHolder configCacheKey, bool ignoreCache = false) {
            if (!ignoreCache) {
                fromContext.TryAdd(configCacheKey, definition.StringValue);
            }
            return (T)Convert.ChangeType(definition.StringValue, typeof(T));
        }


        public System.Collections.Generic.SortedSet<PropertyDefinition> UpdateDefinitions(CategoryDTO category) {
            var definitions = category.Definitions;
            var updatedDefinitions = new System.Collections.Generic.SortedSet<PropertyDefinition>();
            foreach (var def in definitions) {
                //TODO: we are only retrieving again, because a bug where the definition values are not sent over the wire
                var definition = _dao.FindByPK<PropertyDefinition>(typeof(PropertyDefinition), def.FullKey);
                updatedDefinitions.Add(definition);
                string value;
                if (!category.ValuesToSave.TryGetValue(definition.FullKey, out value)) {
                    //we didn´t touch at these definitions
                    continue;
                }
                var condition = category.Condition == null ? null : category.Condition.RealCondition;

                var propValue = _dao.FindSingleByQuery<PropertyValue>(PropertyValue.ByDefinitionConditionModuleProfile, definition.FullKey,
                    condition, category.Module.Id, category.UserProfile);
                if (propValue != null) {
                    UpdateFoundValue(definition, value, propValue);
                } else if (value != null) {
                    propValue = _dao.Save(new PropertyValue {
                        Definition = definition,
                        Module = category.Module.Id,
                        UserProfile = category.UserProfile,
                        Condition = condition,
                        StringValue = value
                    });
                    if (definition.Values == null) {
                        definition.Values = new HashedSet<PropertyValue>();
                    }
                    definition.Values.Add(propValue);
                    ConcurrentDictionary<ContextHolder, string> fromContext;
                    if (ConfigCache.TryGetValue(definition.FullKey, out fromContext)) {
                        //clear entire cache of definion
                        fromContext.Clear();
                    }
                }

            }

            return updatedDefinitions;
        }

        // global property, ignores module, profile and conditions
        public void UpdateGlobalDefinition(string fullKey, string value) {
            var definition = _dao.FindSingleByQuery<PropertyDefinition>(PropertyDefinition.ByKey, fullKey);
            if ("boolean".Equals(definition.DataType) && value != null) {
                value = value.ToLower(); // newtonsoft json workaround it converts bool true to string "True" same for false
            }
            var propValue = InnerGetGlobalPropertyValue(definition);
            if (propValue != null) {
                UpdateFoundValue(definition, value, propValue);
                return;
            }

            if (value == null) {
                return;
            }

            propValue = _dao.Save(new PropertyValue {
                Definition = definition,
                StringValue = value
            });
            if (definition.Values == null) {
                definition.Values = new HashedSet<PropertyValue>();
            }
            definition.Values.Add(propValue);
            _dao.Save(definition);
            UpdateCache(fullKey);
        }

        // global property, ignores module, profile and conditions
        public T GetGlobalPropertyValue<T>(PropertyDefinition definition) {
            var value = InnerGetGlobalPropertyValue(definition);
            var stringValue = value == null ? null : (value.StringValue ?? value.SystemStringValue);
            return (T)Convert.ChangeType(stringValue, typeof(T));
        }

        // global property, ignores module, profile and conditions
        private static PropertyValue InnerGetGlobalPropertyValue(PropertyDefinition definition) {
            var values = definition.Values;
            return values == null || !values.Any() ? null : values.First();
        }

        public IList<PropertyDefinition> GetAllVisibleDefinitions(ConfigTypes type) {
            return _dao.FindByQuery<PropertyDefinition>(PropertyDefinition.ByVisibilityByConfigTypeOrderedByKey, true,
                "/" + type.GetRootLevel() + "/%");
        }

        private void UpdateFoundValue(PropertyDefinition definition, string value, PropertyValue propValue) {
            if (string.IsNullOrEmpty(value) && propValue.SystemValue == null) {
                _dao.Delete(propValue);
                if (definition.Values != null && definition.Values.Contains(propValue)) {
                    definition.Values.Remove(propValue);
                }
            } else {
                propValue.StringValue = value;
                propValue = _dao.Save(propValue);
                if (definition.Values == null) {
                    definition.Values = new HashedSet<PropertyValue>();
                }
                if (definition.Values.Contains(propValue)) {
                    definition.Values.Remove(propValue);
                }
                definition.Values.Add(propValue);
            }
            ConcurrentDictionary<ContextHolder, string> fromContext;
            if (ConfigCache.TryGetValue(definition.FullKey, out fromContext)) {
                //clear entire cache of definion
                fromContext.Clear();
            }
        }
    }
}
