using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using log4net;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Configuration.Util;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Exceptions;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softWrench.sW4.Configuration.Services {

    public class WhereClauseRegisterService : ISingletonComponent {

        private readonly ISWDBHibernateDAO _dao;
        private readonly UserProfileManager _userProfileManager;
        private readonly EntityRepository _entityRepository;
        private readonly ConfigurationCache _configurationCache;

        private static readonly ILog Log = LogManager.GetLogger(typeof(WhereClauseRegisterService));

        public class WhereClauseRegisterResult {
            public WCRegisterOperation Operation {
                get; set;
            }
            public PropertyValue PropertyValue {
                get; set;
            }
        }

        public enum WCRegisterOperation {
            SimpleDefinitionUpdate, ValueUpdate, ValueCreation, Skip, SkipProfileNotFound
        }



        public WhereClauseRegisterService(ISWDBHibernateDAO dao, UserProfileManager userProfileManager, EntityRepository entityRepository, ConfigurationCache configurationCache) {
            _dao = dao;
            _userProfileManager = userProfileManager;
            _entityRepository = entityRepository;
            _configurationCache = configurationCache;
        }

        public virtual void ValidateWhereClause([NotNull]string applicationName, [NotNull]string whereClause, WhereClauseCondition conditionToValidateAgainst = null) {
            if (string.IsNullOrEmpty(whereClause) || whereClause.EqualsAny("1=1", "1!=1")){
                //common whereclauses which validation can be skipped
                return;
            }

            var validators = CustomValidators();
            var customValidator = validators?.FirstOrDefault(validator => validator.DoesValidate(applicationName, conditionToValidateAgainst));
            if (customValidator != null) {
                customValidator.Validate(applicationName, whereClause, conditionToValidateAgainst);
                return;
            }

            var searchRequestDto = new PaginatedSearchRequestDto {
                WhereClause = WhereClauseFacade.BuildWhereClauseResult(whereClause).Query,
                PageSize = 1,
                PageNumber = 1
            };
            var application = MetadataProvider.Application(applicationName, false, true);
            if (application == null) {
                //under some circumstances there will be no applicaiton itself, but rather just a plain entity (e.g commtemplate)
                var entity = MetadataProvider.Entity(applicationName);
                _entityRepository.GetSync(entity, searchRequestDto);
                return;
            }


            if (conditionToValidateAgainst != null && conditionToValidateAgainst.HasSchemaCondition) {
                //TODO: improve schema filtering, to validate only against the proper schema, considering offline conditions, etc
            } else {
                //let´s start by validating all list schemas
                var schemas = application.AllSchemasByStereotype("list");
                try {
                    foreach (var entityMetadata in schemas.Select(MetadataProvider.SlicedEntityMetadata)) {
                        _entityRepository.GetSync(entityMetadata, searchRequestDto);
                    }
                } catch (NHibernate.Exceptions.GenericADOException ex) {
                    throw new InvalidWhereClauseException("Error validating where clause", ex.InnerException);
                } catch (Exception ex) {
                    throw new InvalidWhereClauseException(ex.Message);
                }
            }

        }

        [Transactional(DBType.Swdb)]
        public virtual async Task<PropertyValue> UpdateExisting(int propValueId, string newValue) {
            var propertyValue = await _dao.FindByPKAsync<PropertyValue>(propValueId);
            if (newValue == "") {
                newValue = "1=1";
            }
            propertyValue.Value = newValue;
            return await _dao.SaveAsync(propertyValue);
        }




        [Transactional(DBType.Swdb)]
        public virtual async Task DeleteExisting(int propertyValueId) {
            var propertyValue = await _dao.FindByPKAsync<PropertyValue>(propertyValueId);
            await _dao.DeleteAsync(propertyValue);
        }

        [Transactional(DBType.Swdb)]
        public virtual async Task<PropertyValue> Create(string application, string query, WhereClauseRegisterCondition condition) {
            var configKey = $"/_whereclauses/{application}/whereclause";

            var result = await DoRegister(configKey, query, condition, false,true);
            return result.PropertyValue;
        }


        // [Transactional(DBType.Swdb)]
        //TODO: asyncHelper wont work here with tx
        public virtual async Task<WhereClauseRegisterResult> DoRegister(string configKey, string query, WhereClauseRegisterCondition condition, bool systemValueRegister = true, bool wcCreation =false)
        {
            if (condition != null && condition.Environment != null && condition.Environment != ApplicationConfiguration.Profile) {
                //we don´t need to register this property here.
                return new WhereClauseRegisterResult {
                    Operation = WCRegisterOperation.Skip
                };
            }

            //if the condition is null, we need to apply the default value inside the definition itself
            var definition = GetDefinitionToSave(configKey, query, condition == null);

            var savedDefinition = await _dao.SaveAsync(definition);


            if (condition?.Alias == null && condition?.AppContext?.MetadataId != null) {
                //generating an alias based on the metadataid
                condition.Alias = condition.AppContext.MetadataId;
            }

           
            var storedCondition = await GetStoredCondition(condition, configKey, wcCreation);
          

            var profile = new UserProfile();
            if (condition?.UserProfile != null) {
                profile = _userProfileManager.FindByName(condition.UserProfile);
                if (condition.UserProfile != null && profile == null) {
                    Log.Warn($"unable to register definition as profile {condition.UserProfile} does not exist");
                    return new WhereClauseRegisterResult {
                        Operation = WCRegisterOperation.SkipProfileNotFound
                    };
                }
            } else if (condition?.ProfileId != null) {
                profile.Id = condition.ProfileId;
            }

            var id = storedCondition?.Id;

            PropertyValue storedValue;

            if (condition != null && id!=null) {
                storedValue = await _dao.FindSingleByQueryAsync<PropertyValue>(
                  PropertyValue.ByDefinitionConditionIdModuleProfile,
                  configKey, id, condition.Module, profile.Id);
            } else {
                storedValue = await _dao.FindSingleByQueryAsync<PropertyValue>(PropertyValue.ByDefinitionNoCondition, configKey, profile.Id);
            }


            if (storedValue == null) {
                Log.DebugFormat("creating new PropertyValue for {0}", configKey);
                var newValue = new PropertyValue {
                    Condition = storedCondition,
                    Definition = savedDefinition,
                    Module = condition?.Module,
                    UserProfile = profile.Id,
                    ClientName = ApplicationConfiguration.ClientName,
                };
                if (systemValueRegister) {
                    newValue.SystemStringValue = query;
                } else {
                    newValue.Value = query;
                }

                newValue = await _dao.SaveAsync(newValue);
                return new WhereClauseRegisterResult {
                    Operation = WCRegisterOperation.ValueCreation,
                    PropertyValue = newValue
                };
            }
            Log.DebugFormat("updating existing PropertyValue for {0}", configKey);
            if (systemValueRegister) {
                storedValue.SystemStringValue = query;
            } else {
                storedValue.Value = query;
            }
            storedValue = await _dao.SaveAsync(storedValue);
            //TODO: investigate a way to populate cache immediately
            _configurationCache.ClearCache(configKey);
            return new WhereClauseRegisterResult {
                Operation = WCRegisterOperation.ValueUpdate,
                PropertyValue = storedValue
            };

        }

        internal static PropertyDefinition GetDefinitionToSave(string configKey, string query, bool hasDefaultValue) {
            var definition = new PropertyDefinition {
                FullKey = configKey,
                SimpleKey = CategoryUtil.GetPropertyKey(configKey),
                StringValue = hasDefaultValue ? query : null,
                PropertyDataType = PropertyDataType.STRING,
                Renderer = "whereclause",
                Alias = "",
                Contextualized = true
            };
            return definition;
        }

        private async Task<Condition> GetStoredCondition(WhereClauseRegisterCondition condition,string configKey, bool wcCreation) {

            if (condition == null) {
                return null;
            }
            condition.GenerateAlias();

            //this means that we actually have a condition rather then just a simple utility class WhereClauseRegisterCondition, that could be used for profiles and modules
            var storedCondition = await _dao.FindSingleByQueryAsync<Condition>(Condition.ByAlias, condition.Alias, configKey);
            if (storedCondition != null) {
                if (wcCreation) {
                    throw new InvalidOperationException("The exact same condition is already setup for this application, cannot override it");
                }
                condition.Id = storedCondition.Id;
            }

            var realValue = condition.RealCondition;
            realValue.FullKey = configKey;

            if (realValue.Equals(storedCondition)) {
                Log.DebugFormat("No change on condition, returning existing condition");
                //no need to update or create
                return realValue;
            }

            Log.DebugFormat("updating existing condition");
            //updating existing condition of same alias
            return await _dao.SaveAsync(realValue);

        }

        private static IEnumerable<IWhereClauseValidator> CustomValidators() {
            return SimpleInjectorGenericFactory.Instance.GetObjectsOfType<IWhereClauseValidator>(typeof(IWhereClauseValidator)).ToList();
        }
    }
}
