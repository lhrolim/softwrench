﻿using log4net;
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
using softwrench.sw4.user.classes.entities;

namespace softWrench.sW4.Configuration.Services {
    class WhereClauseFacade : IWhereClauseFacade, ISWEventListener<ApplicationStartedEvent>, IPriorityOrdered {

        private readonly ConfigurationService _configurationService;
        private readonly IContextLookuper _contextLookuper;
        private readonly SWDBHibernateDAO _dao;
        private bool _appStarted;
        private readonly IList<Tuple<string, string, WhereClauseRegisterCondition>> _toRegister = new List<Tuple<string, string, WhereClauseRegisterCondition>>();
        private static readonly ILog Log = LogManager.GetLogger(typeof(WhereClauseFacade));


        private const string WcConfig = "/{0}/{1}/whereclause";
        private const string AppNotFoundEx = "Application/Entity {0} not found, unable to register whereclause";

        public WhereClauseFacade(ConfigurationService configurationService, IContextLookuper contextLookuper, SWDBHibernateDAO dao) {
            _configurationService = configurationService;
            _contextLookuper = contextLookuper;
            _dao = dao;
        }

        public WhereClauseResult Lookup(string applicationName, ApplicationLookupContext lookupContext = null) {
            var context = _contextLookuper.LookupContext();
            if (lookupContext != null) {
                context.ApplicationLookupContext = lookupContext;
            }
            var resultString = _configurationService.Lookup<string>(String.Format(WcConfig, ConfigTypes.WhereClauses.GetRootLevel(), applicationName), context);
            return BuildWhereClauseResult(resultString);
        }

        private static WhereClauseResult BuildWhereClauseResult(string resultString) {
            if (resultString == null) {
                return null;
            }
            resultString = resultString.Trim();
            if (resultString.StartsWith("@")) {
                //@service.method
                var split = resultString.Split('.');
                return new WhereClauseResult {
                    //remove @
                    ServiceName = split[0].Substring(1),
                    MethodName = split[1]
                };
            }
            return new WhereClauseResult { Query = resultString };

        }

        public void Register(string applicationName, String query, WhereClauseRegisterCondition condition = null, bool validate = true) {
            if (validate) {
                Validate(applicationName);
            }
            var configKey = String.Format(WcConfig, ConfigTypes.WhereClauses.GetRootLevel(), applicationName);
            if (!_appStarted) {
                _toRegister.Add(Tuple.Create(configKey, query, condition));
            } else {
                DoRegister(configKey, query, condition);
            }
        }

        private static void Validate(string applicationName) {
            try {
                MetadataProvider.Application(applicationName);
            } catch (Exception) {
                try {
                    MetadataProvider.Entity(applicationName);
                } catch (Exception) {
                    throw new InvalidOperationException(String.Format(AppNotFoundEx, applicationName));
                }
            }
        }

        private void DoRegister(string configKey, string query, WhereClauseRegisterCondition condition) {
            if (condition != null && condition.Environment != null && condition.Environment != ApplicationConfiguration.Profile) {
                //we don´t need to register this property here.
                return;
            }

            if (condition == null) {
                //if no condition is passed, we just need to update the base definition data
                var definition = new PropertyDefinition {
                    FullKey = configKey,
                    Key = CategoryUtil.GetPropertyKey(configKey),
                    StringValue = query,
                    DataType = typeof(string).Name,
                    Renderer = "whereclause",
                    Alias = "",
                    Contextualized = true
                };

                _dao.Save(definition);
                return;
            }

            var savedDefinition = _dao.FindSingleByQuery<PropertyDefinition>(PropertyDefinition.ByKey, configKey);


            Condition storedCondition = null;
            if (condition.Alias != null) {
                //this means that we actually have a condition rather then just a simple utility class WhereClauseRegisterCondition, that could be used for profiles and modules
                storedCondition = _dao.FindSingleByQuery<WhereClauseCondition>(Condition.ByAlias, condition.Alias);
                if (storedCondition != null) {
                    condition.Id = storedCondition.Id;
                }
                storedCondition = _dao.Save(condition.RealCondition);
            }

            var profile = new UserProfile();
            if (condition.UserProfile != null) {
                profile = UserProfileManager.FindByName(condition.UserProfile);
                if (condition.UserProfile != null && profile == null) {
                    Log.Warn(String.Format("unable to register definition as profile {0} does not exist",
                        condition.UserProfile));
                    return;
                }
            }

            var storedValue = _dao.FindSingleByQuery<PropertyValue>(
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
                _dao.Save(newValue);
            } else {
                storedValue.SystemStringValue = query;
                _dao.Save(storedValue);
            }
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            foreach (var entry in _toRegister) {
                DoRegister(entry.Item1, entry.Item2, entry.Item3);
            }
            _appStarted = true;
        }



        //execute last
        public int Order { get { return 100; } }
    }
}
