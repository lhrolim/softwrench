using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Metadata;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {

    public class WoBatchWhereClauseRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private readonly IWhereClauseFacade _wcFacade;

        public WoBatchWhereClauseRegistry(IWhereClauseFacade wcFacade) {
            _wcFacade = wcFacade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (MetadataProvider.Application("workorder", false) == null) return;
            _wcFacade.Register("_wobatch", "userid = @userid", new WhereClauseRegisterCondition() { AppContext = new ApplicationLookupContext() { Schema = "list" } });
            _wcFacade.Register("workorder", "@woBatchWhereClauseProvider.CreateBatchWhereClause", WhereClauseRegisterCondition.ForSchema("createbatchlist"));
        }
    }
}
