using log4net;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.simpleinjector;
using cts.commons.Util;
using Iesi.Collections.Generic;
using NHibernate.Util;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Configuration.Services {
    public class WhereClauseFacade : IWhereClauseFacade, ISWEventListener<ApplicationStartedEvent>, IOrdered {

        private const string DefaultWhereClause = " 1=1 ";

        private readonly ConfigurationService _configurationService;
        private readonly IContextLookuper _contextLookuper;
        private bool _appStarted;
        private readonly WhereClauseRegisterService _whereClauseRegisterService;


        private readonly ConcurrentBag<Tuple<string, string, WhereClauseRegisterCondition>> _toRegister = new ConcurrentBag<Tuple<string, string, WhereClauseRegisterCondition>>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(WhereClauseFacade));


        private const string WcConfig = "/{0}/{1}/whereclause";
        private const string AppNotFoundEx = "Application/Entity {0} not found, unable to register whereclause";

        public WhereClauseFacade(ConfigurationService configurationService, IContextLookuper contextLookuper, WhereClauseRegisterService whereClauseRegisterService) {
            _configurationService = configurationService;
            _contextLookuper = contextLookuper;
            _whereClauseRegisterService = whereClauseRegisterService;
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

        public void ValidateWhereClause(string applicationName, string whereClause, WhereClauseCondition condition = null) {
             _whereClauseRegisterService.ValidateWhereClause(applicationName, whereClause, condition);
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

        [Transactional(DBType.Swdb)]
        public virtual async Task RegisterAsync(string applicationName, string query, WhereClauseRegisterCondition condition = null, bool validate = false) {
            var result = Validate(applicationName,query, validate, condition);
            if (!result) {
                Log.WarnFormat("application {0} not found skipping registration", applicationName);
                return;
            }
            var configKey = GetFullKey(applicationName);
            if (!_appStarted) {
                _toRegister.Add(Tuple.Create(configKey, query, condition));
            } else {
                await _whereClauseRegisterService.DoRegister(configKey, query, condition);
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

        private bool Validate(string applicationName, string whereClause, bool throwException = true, WhereClauseRegisterCondition condition=null) {
            var items = MetadataProvider.FetchAvailableAppsAndEntities();
            if (!items.Contains(applicationName)) {
                if (throwException) {
                    throw new InvalidOperationException(String.Format(AppNotFoundEx, applicationName));
                }
                return false;
            }
            if (!string.IsNullOrEmpty(whereClause) && throwException) {
                ValidateWhereClause(applicationName, whereClause, condition);
            }

            return true;
        }


//        [Transactional(DBType.Swdb)]
        public virtual void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            foreach (var entry in _toRegister) {
                var localEntry = entry;
                AsyncHelper.RunSync(() => _whereClauseRegisterService.DoRegister(localEntry.Item1, localEntry.Item2, localEntry.Item3));
            }
            _appStarted = true;
        }

        //execute last
        public int Order { get { return 100; } }
    }
}
