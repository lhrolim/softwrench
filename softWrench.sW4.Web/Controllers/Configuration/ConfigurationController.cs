using System.Linq;
using System.Web.Security;
using cts.commons.persistence;
using cts.commons.persistence.Event;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Configuration.Util;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Metadata.Modules;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.portable.Util;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Exceptions;

namespace softWrench.sW4.Web.Controllers.Configuration {
    public class ConfigurationController : ApiController {

        private readonly CategoryTreeCache _cache;
        private readonly ConfigurationFacade _configService;
        private readonly I18NResolver _resolver;
        private readonly SWDBHibernateDAO _dao;
        private readonly ConditionService _conditionService;
        private readonly IConfigurationFacade _facade;
        private readonly SimpleInjectorDomainEventDispatcher _dispatcher;

        private readonly IWhereClauseFacade _whereClauseFacade;



        public ConfigurationController(CategoryTreeCache cache, ConfigurationFacade configService, I18NResolver resolver, SWDBHibernateDAO dao, ConditionService conditionService, IConfigurationFacade facade,
            SimpleInjectorDomainEventDispatcher dispatcher, IWhereClauseFacade whereClauseFacade) {
            _cache = cache;
            _configService = configService;
            _resolver = resolver;
            _dao = dao;
            _conditionService = conditionService;
            _facade = facade;
            _dispatcher = dispatcher;
            _whereClauseFacade = whereClauseFacade;
        }

        [SPFRedirect("Configuration", "_headermenu.configuration")]
        public IGenericResponseResult Get() {
            var rootEntities = _cache.GetCache(ConfigTypes.Global);
            var result = new ConfigurationScreenResult() {
                Categories = rootEntities,
                //                Modules = MetadataProvider.Modules(ClientPlatform.Web),
                //                Profiles = SecurityFacade.GetInstance().FetchAllProfiles(false),
                Type = ConfigTypes.Global,
                Conditions = _dao.FindByQuery<Condition>(Condition.GlobalConditions),
            };
            return new GenericResponseResult<ConfigurationScreenResult>(result);
        }

        [SPFRedirect("Where Clauses", "_headermenu.whereclauses")]
        [HttpGet]
        public IGenericResponseResult WhereClauses() {
            var rootEntities = _cache.GetCache(ConfigTypes.WhereClauses);
            var names = MetadataProvider.FetchAvailableAppsAndEntities(false);
            var applications = names.Select(name => new GenericAssociationOption(name, name)).Cast<IAssociationOption>().ToList().OrderBy(a => a.Label);
            //            applications.s

            var result = new ConfigurationScreenResult() {
                Categories = rootEntities,
                Modules = _cache.GetConfigModules(),
                Profiles = SecurityFacade.GetInstance().FetchAllProfiles(false),
                Type = ConfigTypes.WhereClauses,
                Applications = applications,
                Conditions = _dao.FindAll<Condition>(typeof(Condition)),
            };
            return new GenericResponseResult<ConfigurationScreenResult>(result);
        }

        /// <summary>
        /// Validates and adds a new where clause for a application entity 
        /// </summary>
        /// <param name="category">The <see cref="CategoryDTO"/> object</param>
        /// <returns>the <see cref="IGenericResponseResult"/> result</returns>
        /// <exception cref="InvalidWhereClauseException">Thrown when an invalid where clause is supplied</exception>
        public async Task<IGenericResponseResult> Put(CategoryDTO category) {
            var whereClauses = category.Definitions.Where(definition => category.FullKey.StartsWith("/_whereclauses/"));
            foreach (var definition in whereClauses) {
                //not all categories are related to whereclauses only these need to be validated
                string value;
                if (category.ValuesToSave.TryGetValue(definition.FullKey, out value)) {
                    _whereClauseFacade.ValidateWhereClause(category.Key, value, category.Condition);
                }
            }

            var result = await _configService.UpdateDefinitions(category);
            category.Definitions = result;
            var updatedCategories = _cache.Update(category);
            var response = new GenericResponseResult<SortedSet<CategoryDTO>> {
                ResultObject = updatedCategories,
                SuccessMessage = "Configuration successfully saved"
            };
            return response;
        }


        //
        //        [HttpPut]
        //        public IGenericResponseResult CreateCondition(WhereClauseRegisterCondition condition) {
        //            var realCondition = condition.RealCondition;
        //            if (realCondition == null) {
        //                throw new InvalidOperationException("error creating condition");
        //            }
        //            var storedCondition = _dao.Save(realCondition);
        //            _facade.ConditionAltered(storedCondition.FullKey + "whereclause");
        //            return new GenericResponseResult<Condition> {
        //                ResultObject = storedCondition,
        //                SuccessMessage = "Condition successfully created"
        //            };
        //        }

        [HttpPut]
        public IGenericResponseResult CreateGlobalCondition(string metadataid, string alias, bool offlineOnly) {
            var realCondition = new WhereClauseCondition {
                AppContext = new sW4.Security.Context.ApplicationLookupContext {
                    MetadataId = metadataid
                },
                Alias = alias,
                OfflineOnly = offlineOnly,
                Global = true
            };
            var storedCondition = _dao.Save(realCondition);
            //            _facade.ConditionAltered(storedCondition.FullKey + "whereclause");
            return new GenericResponseResult<Condition> {
                ResultObject = storedCondition,
                SuccessMessage = "Condition successfully created"
            };
        }


        [HttpPost]
        public async Task<IGenericResponseResult> EditGlobalCondition(int id, string metadataid, string alias, bool offlineOnly) {

            var wcCondition = await _dao.FindByPKAsync<WhereClauseCondition>(id);
            wcCondition.Alias = alias;
            wcCondition.AppContext = new sW4.Security.Context.ApplicationLookupContext {
                MetadataId = metadataid
            };
            wcCondition.OfflineOnly = offlineOnly;
            var storedCondition = _dao.Save(wcCondition);
            //            _facade.ConditionAltered(storedCondition.FullKey + "whereclause");
            return new GenericResponseResult<Condition> {
                ResultObject = storedCondition,
                SuccessMessage = "Condition {0} successfully updated".Fmt(alias)
            };
        }

        [HttpPut]
        public IGenericResponseResult DeleteCondition(WhereClauseRegisterCondition condition, [FromUri]string currentKey) {
            var updatedConditions = _conditionService.RemoveCondition(condition, currentKey);
            _facade.ConditionAltered(condition.FullKey + "whereclause");
            return new GenericResponseResult<ICollection<Condition>> {
                ResultObject = updatedConditions
            };
        }

        [HttpGet]
        [SPFRedirect("About", "about.title", "About")]
        public GenericResponseResult<IList<KeyValuePair<string, string>>> About() {

            IList<KeyValuePair<string, string>> aboutData = new List<KeyValuePair<string, string>>();

            var maximoDB = ApplicationConfiguration.DBConnectionStringBuilder(DBType.Maximo);
            var swDB = ApplicationConfiguration.DBConnectionStringBuilder(DBType.Swdb);

            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.version", "Version"), ApplicationConfiguration.SystemVersion));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.revision", "Revision"), ApplicationConfiguration.SystemRevision));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.builddate", "Build Date"), ApplicationConfiguration.SystemBuildDate.ToString(CultureInfo.InvariantCulture.DateTimeFormat)));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.clientname", "Client Name"), ApplicationConfiguration.ClientName));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.profile", "Profile"), ApplicationConfiguration.Profile));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.maximourl", "Maximo URL"), _configService.Lookup<string>(ConfigurationConstants.Maximo.WsdlPath, "baseWSPrefix")));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("about.maximodb", "Maximo DB"),
                $"{maximoDB.DataSource}/{maximoDB.Catalog}"));
            aboutData.Add(new KeyValuePair<string, string>(_resolver.I18NValue("maximodb.version", "SW DB"),
                $"{swDB.DataSource}/{swDB.Catalog}"));

            return new GenericResponseResult<IList<KeyValuePair<string, string>>>(aboutData);
        }

        [HttpPost]
        public void ChangeClient(string clientName) {
            _dispatcher.Dispatch(new ClearCacheEvent());
            _dispatcher.Dispatch(new ClientChangeEvent(clientName, false));
            _dispatcher.Dispatch(new RestartDBEvent());
            FormsAuthentication.SignOut();
        }

        [HttpPost]
        public void Restore() {
            _dispatcher.Dispatch(new ClearCacheEvent());
            _dispatcher.Dispatch(new ClientChangeEvent(null, true));
            _dispatcher.Dispatch(new RestartDBEvent());
            FormsAuthentication.SignOut();
        }

        [HttpPost]
        public async Task SetConfiguration(string fullKey, string value) {
            await _facade.SetValue(fullKey, value);
        }

        [HttpGet]
        public string GetConfiguration(string fullKey) {
            var config = _facade.Lookup<string>(fullKey);
            return config;
        }

        [HttpGet]
        public IDictionary<string, string> GetConfigurations([FromUri]List<string> fullKeys) {
            var configs = new Dictionary<string, string>();
            if (fullKeys == null || fullKeys.Count == 0) {
                return configs;
            }
            fullKeys.ForEach(k => configs.Add(k, _facade.Lookup<string>(k)));
            return configs;
        }

        [HttpGet]
        public async Task<ClientSideConfigurations> GetClientSideConfigurations([FromUri] long? cacheTimestamp) {
            return await _facade.GetClientSideConfigurations(cacheTimestamp);
        }

        class ConfigurationScreenResult {

            public IEnumerable<ModuleDefinition> Modules {
                get; set;
            }
            public ICollection<UserProfile> Profiles {
                get; set;
            }

            public IOrderedEnumerable<IAssociationOption> Applications {
                get; set;
            }

            public SortedSet<CategoryDTO> Categories {
                get; set;
            }

            public ConfigTypes Type {
                get; set;
            }

            public ICollection<Condition> Conditions {
                get; set;
            }
        }
    }
}