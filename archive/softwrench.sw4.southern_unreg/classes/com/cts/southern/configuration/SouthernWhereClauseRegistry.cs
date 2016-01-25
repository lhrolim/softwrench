using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Entities.SyncManagers;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;

namespace softwrench.sw4.southern_unreg.classes.com.cts.southern.configuration {
    public class SouthernWhereClauseRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {

        private readonly IWhereClauseFacade _facade;

        public SouthernWhereClauseRegistry(IWhereClauseFacade facade) {
            _facade = facade;
        }

        private void RegisterQueries() {
            _facade.Register("person", "person.personid in (select personid from maxuser where userid in (select userid from  GROUPUSER where groupname = 'C2_SOFTWRENCH'))", new WhereClauseRegisterCondition {
                Alias = SwUserConstants.PersonUserMetadataId,
                AppContext = new ApplicationLookupContext { MetadataId = SwUserConstants.PersonUserMetadataId }
            });
        }

        public void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (!ApplicationConfiguration.ClientName.Equals("southern_unreg")) {
                return;
            }

//            RegisterQueries();
        }
    }
}
