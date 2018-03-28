using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Util;

namespace softwrench.sw4.swgas.classes.swgas.cts.swgas.configuration {

    public class SwgasWhereClauseRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private readonly IWhereClauseFacade _whereClauseFacade;

        public SwgasWhereClauseRegistry(IWhereClauseFacade whereClauseFacade) {
            _whereClauseFacade = whereClauseFacade;
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (!ApplicationConfiguration.IsClient("swgas")) {
                //to avoid issues on dev environments
                return;
            }
            var offLineCondition = new WhereClauseRegisterCondition() { Alias = "offline", OfflineOnly = true, Global = true };
            _whereClauseFacade.Register("workorder", "historyflag = 0 and status in ('WAPPR','APPR','INPRG')", offLineCondition);
        }
    }
}
