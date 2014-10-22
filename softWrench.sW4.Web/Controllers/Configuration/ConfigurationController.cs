﻿using softWrench.sW4.Configuration;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Configuration.Util;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sw4.Shared2.Metadata.Modules;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;
using softWrench.sW4.Web.SPF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Web.Http;

namespace softWrench.sW4.Web.Controllers.Configuration {
    public class ConfigurationController : ApiController {

        private readonly CategoryTreeCache _cache;
        private readonly ConfigurationService _configService;
        private readonly I18NResolver _resolver;
        private readonly SWDBHibernateDAO _dao;
        private readonly ConditionService _conditionService;
        private readonly IConfigurationFacade _facade;


        public ConfigurationController(CategoryTreeCache cache, ConfigurationService configService, I18NResolver resolver, SWDBHibernateDAO dao, ConditionService conditionService, IConfigurationFacade facade) {
            _cache = cache;
            _configService = configService;
            _resolver = resolver;
            _dao = dao;
            _conditionService = conditionService;
            _facade = facade;
        }

        [SPFRedirect("Configuration", "_headermenu.configuration")]
        public IGenericResponseResult Get() {
            var rootEntities = _cache.GetCache(ConfigTypes.Global);
            var result = new ConfigurationScreenResult() {
                Categories = rootEntities,
                //                Modules = MetadataProvider.Modules(ClientPlatform.Web),
                //                Profiles = SecurityFacade.GetInstance().FetchAllProfiles(false),
                Type = ConfigTypes.Global,
                Conditions = _dao.FindByQuery<Condition>(Condition.GlobalConditions)
            };
            return new GenericResponseResult<ConfigurationScreenResult>(result);
        }

        [SPFRedirect("Where Clauses", "_headermenu.whereclauses")]
        [HttpGet]
        public IGenericResponseResult WhereClauses() {
            var rootEntities = _cache.GetCache(ConfigTypes.WhereClauses);
            var result = new ConfigurationScreenResult() {
                Categories = rootEntities,
                Modules = _cache.GetConfigModules(),
                Profiles = SecurityFacade.GetInstance().FetchAllProfiles(false),
                Type = ConfigTypes.WhereClauses,
                Conditions = _dao.FindAll<Condition>(typeof(Condition))
            };
            return new GenericResponseResult<ConfigurationScreenResult>(result);
        }


        public IGenericResponseResult Put(CategoryDTO category) {
            var result = _configService.UpdateDefinitions(category);
            category.Definitions = result;
            var updatedCategories =_cache.Update(category);
            var response = new GenericResponseResult<SortedSet<CategoryDTO>> {
                ResultObject = updatedCategories,
                SuccessMessage = "Configuration successfully saved"
            };
            return response;
        }

        [HttpPut]
        public IGenericResponseResult CreateCondition(WhereClauseRegisterCondition condition) {
            var realCondition = condition.RealCondition;
            if (realCondition == null) {
                throw new InvalidOperationException("error creating condition");
            }
            var storedCondition = _dao.Save(realCondition);
            return new GenericResponseResult<Condition> {
                ResultObject = storedCondition,
                SuccessMessage = "Condition successfully created"
            };
        }

        [HttpPut]
        public IGenericResponseResult DeleteCondition(WhereClauseRegisterCondition condition, [FromUri]string currentKey) {
            var updatedConditions = _conditionService.RemoveCondition(condition, currentKey);
            return new GenericResponseResult<ICollection<Condition>> {
                ResultObject = updatedConditions
            };
        }

        [HttpGet]
        [SPFRedirect("About softWrench", "about.title", "About")]
        public GenericResponseResult<IList<KeyValuePair<String, String>>> About() {

            IList<KeyValuePair<String, String>> aboutData = new List<KeyValuePair<String, String>>();

            var maximoDB = ApplicationConfiguration.DBConnectionStringBuilder(ApplicationConfiguration.DBType.Maximo);
            var swDB = ApplicationConfiguration.DBConnectionStringBuilder(ApplicationConfiguration.DBType.Swdb);

            aboutData.Add(new KeyValuePair<String, String>(_resolver.I18NValue("about.version", "Version"), ApplicationConfiguration.SystemVersion));
            aboutData.Add(new KeyValuePair<String, String>(_resolver.I18NValue("about.revision", "Revision"), ApplicationConfiguration.SystemRevision));
            aboutData.Add(new KeyValuePair<String, String>(_resolver.I18NValue("about.builddate", "Build Date"), ApplicationConfiguration.SystemBuildDate.ToString(CultureInfo.InvariantCulture.DateTimeFormat)));
            aboutData.Add(new KeyValuePair<String, String>(_resolver.I18NValue("about.clientname", "Client Name"), ApplicationConfiguration.ClientName));
            aboutData.Add(new KeyValuePair<String, String>(_resolver.I18NValue("about.profile", "Profile"), ApplicationConfiguration.Profile));
            aboutData.Add(new KeyValuePair<String, String>(_resolver.I18NValue("about.maximourl", "Maximo URL"), ApplicationConfiguration.WsUrl));
            aboutData.Add(new KeyValuePair<String, String>(_resolver.I18NValue("about.maximodb", "Maximo DB"), String.Format("{0}/{1}", maximoDB.DataSource, maximoDB.Catalog)));
            aboutData.Add(new KeyValuePair<String, String>(_resolver.I18NValue("maximodb.version", "SW DB"), String.Format("{0}/{1}", swDB.DataSource, swDB.Catalog)));

            return new GenericResponseResult<IList<KeyValuePair<String, String>>>(aboutData);
        }

        [HttpPost]
        public void SetConfiguration(string fullKey, string value) {
            _facade.SetValue(fullKey, value);
        }

        [HttpGet]
        public string GetConfiguration(string fullKey) {
            var config = _facade.Lookup<string>(fullKey);
            return config;
        }

        class ConfigurationScreenResult {
            public IEnumerable<ModuleDefinition> Modules { get; set; }
            public ICollection<UserProfile> Profiles { get; set; }

            public SortedSet<CategoryDTO> Categories { get; set; }

            public ConfigTypes Type { get; set; }

            public ICollection<Condition> Conditions { get; set; }
        }

    }
}