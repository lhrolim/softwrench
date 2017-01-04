using System.Diagnostics;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.Util;
using log4net;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Metadata;
using cts.commons.simpleinjector.Core.Order;
using cts.commons.simpleinjector.Events;

namespace softWrench.sW4.Configuration.Services {
    public class ApplicationWhereClauseRegistry : ISWEventListener<ApplicationStartedEvent>, IPriorityOrdered {

        private readonly IWhereClauseFacade _facade;

        private readonly ILog _log = LogManager.GetLogger(typeof(ApplicationWhereClauseRegistry));

        public ApplicationWhereClauseRegistry(IWhereClauseFacade facade) {
            _facade = facade;
        }

        [Transactional(DBType.Swdb)]
        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var before = new Stopwatch();
            var applications = MetadataProvider.Applications(true);
            System.Collections.Generic.ISet<string> namesToRegister = MetadataProvider.FetchAvailableAppsAndEntities();
            foreach (var name in namesToRegister) {
                _facade.Register(name, "", null, false);
            }
            _log.Info(LoggingUtil.BaseDurationMessage("finished registering whereclauses in {0}", before));
        }

        public int Order { get { return -1; } }
    }
}
