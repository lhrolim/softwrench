using log4net;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Configuration.Util;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.Util;
using Iesi.Collections.Generic;
using NHibernate.Util;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Configuration.Services {
    public class WhereClauseFacade : IWhereClauseFacade, ISWEventListener<ApplicationStartedEvent>, IPriorityOrdered {

        private const string DefaultWhereClause = " 1=1 ";

        private readonly ConfigurationService _configurationService;
        private readonly IContextLookuper _contextLookuper;
        private readonly SWDBHibernateDAO _dao;
        private bool _appStarted;

        private readonly UserProfileManager _userProfileManager;


        private readonly IList<Tuple<string, string, WhereClauseRegisterCondition>> _toRegister = new List<Tuple<string, string, WhereClauseRegisterCondition>>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(WhereClauseFacade));


        private const string WcConfig = "/{0}/{1}/whereclause";
        private const string AppNotFoundEx = "Application/Entity {0} not found, unable to register whereclause";

        public WhereClauseFacade(ConfigurationService configurationService, IContextLookuper contextLookuper, SWDBHibernateDAO dao, UserProfileManager userProfileManager) {
            _configurationService = configurationService;
            _contextLookuper = contextLookuper;
            _dao = dao;
            _userProfileManager = userProfileManager;
        }

        public WhereClauseResult Lookup(string applicationName, ApplicationLookupContext lookupContext = null, ContextHolder contextHolder = null) {
            return AsyncHelper.RunSync(() => LookupAsync(applicationName, lookupContext, contextHolder));
        }

        public async Task<WhereClauseResult> LookupAsync(string applicationName, ApplicationLookupContext lookupContext = null, ContextHolder contextHolder = null) {
            var context = _contextLookuper.LookupContext();
            if (contextHolder != null) {
                context = contextHolder;
            }
            if (lookupContext != null) {
                context.ApplicationLookupContext = lookupContext;
            }
            var resultString = await _configurationService.Lookup<string>(GetFullKey(applicationName), context);
            return BuildWhereClauseResult(resultString);
        }

        /// <summary>
        /// Builds a where clause result 
        /// </summary>
        /// <param name="resultString">The result string</param>
        /// <returns>The <see cref="WhereClauseResult"/> object.</returns>
        public static WhereClauseResult BuildWhereClauseResult(string resultString) {
            if (resultString == null) {
                return null;
            }
            resultString = resultString.Trim();
            WhereClauseResult result;
            if (resultString.StartsWith("@")) {
                //@service.method
                var split = resultString.Split('.');
                result = new WhereClauseResult {
                    //remove @
                    ServiceName = split[0].Substring(1),
                    MethodName = split[1]
                };
            } else {
                result = new WhereClauseResult { Query = resultString };
            }

            result.Query = GetConvertedWhereClause(result, SecurityFacade.CurrentUser());
            return result;
        }

        private static string GetConvertedWhereClause(WhereClauseResult whereClauseResult, InMemoryUser user, string defaultValue = DefaultWhereClause) {
            if (!string.IsNullOrEmpty(whereClauseResult.Query)) {
                return DefaultValuesBuilder.ConvertAllValues(whereClauseResult.Query, user);
            }
            if (!string.IsNullOrEmpty(whereClauseResult.ServiceName)) {
                var ob = SimpleInjectorGenericFactory.Instance.GetObject<object>(whereClauseResult.ServiceName);
                if (ob != null) {
                    var result = ReflectionUtil.Invoke(ob, whereClauseResult.MethodName, new object[] { });
                    if (!(result is string)) {
                        return DefaultWhereClause;
                    }
                    return DefaultValuesBuilder.ConvertAllValues((string)result, user);
                }
            }
            return defaultValue;
        }

        public void Register(string applicationName, String query, WhereClauseRegisterCondition condition = null, bool validate = false) {
            AsyncHelper.RunSync(() => RegisterAsync(applicationName, query, condition, validate));
        }

        public async Task RegisterAsync(string applicationName, String query, WhereClauseRegisterCondition condition = null, bool validate = false) {

            var result = Validate(applicationName, validate);
            if (!result) {
                Log.WarnFormat("application {0} not found skipping registration", applicationName);
                return;
            }
            var configKey = GetFullKey(applicationName);
            if (!_appStarted) {
                _toRegister.Add(Tuple.Create(configKey, query, condition));
            } else {
                await DoRegister(configKey, query, condition);
            }
        }

        public async Task<ISet<UserProfile>> ProfilesByApplication(string applicationName, InMemoryUser loggedUser) {

            var profiles = loggedUser.Profiles;
            if (!EnumerableExtensions.Any(profiles)) {
                //no profiles at all, nothing to consider
                return new LinkedHashSet<UserProfile>();
            }
            int? defaultId = null;
            var sb = new StringBuilder();
            var result = new List<UserProfile>();
            foreach (var profile in profiles) {

                if (!profile.HasApplicationPermission(applicationName)) {
                    //if the profile has no permissions over the application there´s no point adding it to this list
                    continue;
                }

                var holder = new ContextHolder {
                    CurrentSelectedProfile = profile.Id,
                    UserProfiles = new SortedSet<int?>(loggedUser.ProfileIds)
                };
                var wc = await _configurationService.Lookup<string>(GetFullKey(applicationName), holder);
                if (!string.IsNullOrEmpty(wc)) {
                    result.Add(profile);
                } else {
                    sb.Append(profile.Name).Append(" | ");
                    defaultId = profile.Id;
                }
            }
            if (EnumerableExtensions.Any(result) && defaultId != null) {
                result.Insert(0, new UserProfile {
                    Id = defaultId,
                    Name = sb.ToString(0, sb.Length - 3)
                });

            }

            return new LinkedHashSet<UserProfile>(result);
        }

        private static string GetFullKey(string applicationName) {
            return string.Format(WcConfig, ConfigTypes.WhereClauses.GetRootLevel(), applicationName.ToLower());
        }

        private static bool Validate(string applicationName, bool throwException = true) {
            var items = MetadataProvider.FetchAvailableAppsAndEntities();
            if (!items.Contains(applicationName)) {
                if (throwException) {
                    throw new InvalidOperationException(String.Format(AppNotFoundEx, applicationName));
                }
                return false;
            }
            return true;
        }

        private async Task DoRegister(string configKey, string query, WhereClauseRegisterCondition condition) {



            if (condition != null && condition.Environment != null && condition.Environment != ApplicationConfiguration.Profile) {
                //we don´t need to register this property here.
                return;
            }

            if (condition == null) {
                //if no condition is passed, we just need to update the base definition data
                var definition = new PropertyDefinition {
                    FullKey = configKey,
                    SimpleKey = CategoryUtil.GetPropertyKey(configKey),
                    StringValue = query,
                    DataType = typeof(string).Name,
                    Renderer = "whereclause",
                    Alias = "",
                    Contextualized = true
                };

                await _dao.SaveAsync(definition);
                return;
            }

            var savedDefinition = await _dao.FindSingleByQueryAsync<PropertyDefinition>(PropertyDefinition.ByKey, configKey);


            Condition storedCondition = null;

            if (condition.Alias == null && condition.AppContext != null && condition.AppContext.MetadataId != null) {
                condition.Alias = condition.AppContext.MetadataId;
            }

            if (condition.Alias != null) {
                //this means that we actually have a condition rather then just a simple utility class WhereClauseRegisterCondition, that could be used for profiles and modules
                storedCondition = await _dao.FindSingleByQueryAsync<WhereClauseCondition>(Condition.ByAlias, condition.Alias);
                if (storedCondition != null) {
                    condition.Id = storedCondition.Id;
                }
                storedCondition = await _dao.SaveAsync(condition.RealCondition);
            }

            var profile = new UserProfile();
            if (condition.UserProfile != null) {
                profile = _userProfileManager.FindByName(condition.UserProfile);
                if (condition.UserProfile != null && profile == null) {
                    Log.Warn(String.Format("unable to register definition as profile {0} does not exist",
                        condition.UserProfile));
                    return;
                }
            }

            var storedValue = await _dao.FindSingleByQueryAsync<PropertyValue>(
                  PropertyValue.ByDefinitionConditionModuleProfile,
                  savedDefinition.FullKey, storedCondition, condition.Module, profile.Id);

            if (storedValue == null) {
                var newValue = new PropertyValue {
                    Condition = storedCondition,
                    Definition = savedDefinition,
                    SystemStringValue = query,
                    Module = condition.Module,
                    UserProfile = profile.Id
                };
                await _dao.SaveAsync(newValue);
            } else {
                storedValue.SystemStringValue = query;
                await _dao.SaveAsync(storedValue);
            }
        }

        public async void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            foreach (var entry in _toRegister) {
                await DoRegister(entry.Item1, entry.Item2, entry.Item3);
            }
            _appStarted = true;
        }



        //execute last
        public int Order {
            get {
                return 100;
            }
        }
    }
}
