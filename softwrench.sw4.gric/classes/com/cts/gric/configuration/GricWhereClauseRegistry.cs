using cts.commons.simpleinjector.Events;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.simpleinjector;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Util;

namespace softwrench.sw4.gric.classes.com.cts.gric.configuration {
    public class GricWhereClauseRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private readonly IWhereClauseFacade _whereClauseFacade;

        public GricWhereClauseRegistry(IWhereClauseFacade whereClauseFacade) {
            _whereClauseFacade = whereClauseFacade;
        }

        [Transactional(DBType.Swdb)]
        public virtual void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (!ApplicationConfiguration.IsClient("gric")) {
                //to avoid issues on dev environments
                return;
            }

            var offLineCondition = new WhereClauseRegisterCondition() { Alias = "offline", OfflineOnly = true, Global = true };

            _whereClauseFacade.Register("workorder", "workorder.status not in ('comp','close')", offLineCondition);
        }
    }
}
