using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Security.Context;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.SimpleInjector.Events;
using softWrench.sW4.Util;

namespace softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.workorder {

    public class WoBatchWhereClauseRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private readonly IWhereClauseFacade _wcFacade;

        public WoBatchWhereClauseRegistry(IWhereClauseFacade wcFacade) {
            _wcFacade = wcFacade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            var clientName = ApplicationConfiguration.ClientName;
            if(clientName=="otb"){
                _wcFacade.Register("_wobatch", "userid = @userid", new WhereClauseRegisterCondition() { AppContext = new ApplicationLookupContext() { Schema = "list" } });
                }
            _wcFacade.Register("workorder", "@workOrderBatchWhereClauseProvider.CreateBatchWhereClause",  WhereClauseRegisterCondition.ForSchema("createbatchlist"));
        }
    }
}
