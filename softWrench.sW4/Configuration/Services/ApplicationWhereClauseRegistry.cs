using System;
using System.Diagnostics;
using System.Linq;
using cts.commons.Util;
using Iesi.Collections.Generic;
using log4net;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Metadata;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;

namespace softWrench.sW4.Configuration.Services {
    public class ApplicationWhereClauseRegistry : ISWEventListener<ApplicationStartedEvent>, IPriorityOrdered {

        private readonly IWhereClauseFacade _facade;

        private readonly ILog _log = LogManager.GetLogger(typeof(ApplicationWhereClauseRegistry));

        public ApplicationWhereClauseRegistry(IWhereClauseFacade facade) {
            _facade = facade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var before = new Stopwatch();
            var applications = MetadataProvider.Applications(true);
            var completeApplicationMetadataDefinitions = applications as CompleteApplicationMetadataDefinition[] ?? applications.ToArray();
            System.Collections.Generic.ISet<string> namesToRegister = MetadataProvider.FetchAvailableAppsAndEntities();
            foreach (var name in namesToRegister) {
                _facade.Register(name, "", null, false);
            }
            _log.Info(LoggingUtil.BaseDurationMessage("finished registering whereclauses in {0}", before));
        }

        private static void AddAllApplicationsAndUsedEntities(ISet<string> namesToRegister,
            CompleteApplicationMetadataDefinition[] completeApplicationMetadataDefinitions) {
            foreach (var application in MetadataProvider.Applications(true)) {
                namesToRegister.Add(application.ApplicationName);
                foreach (var schema in application.Schemas()) {
                    foreach (var association in schema.Value.Associations()) {
                        var entityName = association.EntityAssociation.To;
                        var associationApplication =
                            completeApplicationMetadataDefinitions.FirstOrDefault(a => a.Entity == entityName);
                        var toAdd = associationApplication == null ? association.EntityAssociation.To : associationApplication.ApplicationName;
                        namesToRegister.Add(toAdd);
                    }
                }
            }
        }

        public int Order { get { return -1; } }
    }
}
