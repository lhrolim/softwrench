﻿using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using log4net;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Configuration.Util;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Configuration.Services {

    public class WhereClauseRegisterService : ISingletonComponent {

        private readonly ISWDBHibernateDAO _dao;
        private readonly UserProfileManager _userProfileManager;

        private static readonly ILog Log = LogManager.GetLogger(typeof(WhereClauseRegisterService));

        public enum WCRegisterOperation {
            SimpleDefinitionUpdate, ValueUpdate, ValueCreation, Skip, SkipProfileNotFound
        }



        public WhereClauseRegisterService(ISWDBHibernateDAO dao, UserProfileManager userProfileManager) {
            _dao = dao;
            _userProfileManager = userProfileManager;
        }

        public async Task<WCRegisterOperation> DoRegister(string configKey, string query, WhereClauseRegisterCondition condition) {

            if (condition != null && condition.Environment != null && condition.Environment != ApplicationConfiguration.Profile) {
                //we don´t need to register this property here.
                return WCRegisterOperation.Skip;
            }

            //if the condition is null, we need to apply the default value inside the definition itself
            var definition = GetDefinitionToSave(configKey, query,  condition==null);

            var savedDefinition = await _dao.SaveAsync(definition);

            if (condition == null) {
                Log.DebugFormat("No Condition: just updating base definition data");
                //if no condition is passed, we just need to update the base definition data
                return WCRegisterOperation.SimpleDefinitionUpdate;
            }


            if (condition.Alias == null && condition.AppContext != null && condition.AppContext.MetadataId != null) {
                //generating an alias based on the metadataid
                condition.Alias = condition.AppContext.MetadataId;
            }

            var storedCondition = await GetStoredCondition(condition);

            var profile = new UserProfile();
            if (condition.UserProfile != null) {
                profile = _userProfileManager.FindByName(condition.UserProfile);
                if (condition.UserProfile != null && profile == null) {
                    Log.Warn(string.Format("unable to register definition as profile {0} does not exist", condition.UserProfile));
                    return WCRegisterOperation.SkipProfileNotFound;
                }
            }

            var id = storedCondition == null ? null : storedCondition.Id;

            var storedValue = await _dao.FindSingleByQueryAsync<PropertyValue>(
                  PropertyValue.ByDefinitionConditionIdModuleProfile,
                  configKey, id, condition.Module, profile.Id);

            if (storedValue == null) {
                Log.DebugFormat("creating new PropertyValue for {0}", configKey);
                var newValue = new PropertyValue {
                    Condition = storedCondition,
                    Definition = savedDefinition,
                    SystemStringValue = query,
                    Module = condition.Module,
                    UserProfile = profile.Id
                };
                await _dao.SaveAsync(newValue);
                return WCRegisterOperation.ValueCreation;
            }
            Log.DebugFormat("updating existing PropertyValue for {0}", configKey);
            storedValue.SystemStringValue = query;
            await _dao.SaveAsync(storedValue);
            return WCRegisterOperation.ValueUpdate;

        }

        internal static PropertyDefinition GetDefinitionToSave(string configKey, string query, bool hasDefaultValue) {
            var definition = new PropertyDefinition {
                FullKey = configKey,
                SimpleKey = CategoryUtil.GetPropertyKey(configKey),
                StringValue = hasDefaultValue ? query : null,
                DataType = typeof(string).Name,
                Renderer = "whereclause",
                Alias = "",
                Contextualized = true
            };
            return definition;
        }

        private async Task<Condition> GetStoredCondition(WhereClauseRegisterCondition condition) {

            if (condition.Alias == null) {
                return null;
            }

            //this means that we actually have a condition rather then just a simple utility class WhereClauseRegisterCondition, that could be used for profiles and modules
            var storedCondition = await _dao.FindSingleByQueryAsync<Condition>(Condition.ByAlias, condition.Alias);
            if (storedCondition != null) {
                condition.Id = storedCondition.Id;
            }

            var realValue = condition.RealCondition;

            if (realValue.Equals(storedCondition)) {
                Log.DebugFormat("No change on condition, returning existing condition");
                //no need to update or create
                return realValue;
            }

            Log.DebugFormat("updating existing condition");
            //updating existing condition of same alias
            return (WhereClauseCondition)await _dao.SaveAsync(realValue);

        }
    }
}
