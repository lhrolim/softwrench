using System;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Security.Init;
using softWrench.sW4.Security.Init.Com;
using softWrench.sW4.Util;

namespace softWrench.sW4.Data.Configuration {
    public class ComConfigurationRegistry : ISingletonComponent, ISWEventListener<ApplicationStartedEvent> {
        private readonly IWhereClauseFacade _wcFacade;
        private readonly IConfigurationFacade _facade;

        public ComConfigurationRegistry(IConfigurationFacade facade, IWhereClauseFacade wcFacade, SWDBHibernateDAO dao) {
            _wcFacade = wcFacade;
            _facade = facade;
        }

        [Transactional(DBType.Swdb)]
        public virtual void HandleEvent(ApplicationStartedEvent eventToDispatch) {
            if (ApplicationConfiguration.ClientName != "manchester") {
                return;
            }
            CreateBaseWhereClauses();
        }

        private void CreateBaseWhereClauses() {
            _wcFacade.Register("servicerequest", ComWhereClause("SrGridQuery"), ForProfile(ProfileType.DefaultComUsers));
            _wcFacade.Register("workorder", ComWhereClause("WorkOrderGridQuery"), ForProfile(ProfileType.DefaultComUsers));
        }

        private static WhereClauseRegisterCondition ForProfile(ProfileType profile) {
            return new WhereClauseRegisterCondition {
                UserProfile = profile.GetName(),
            };
        }

        private string ComWhereClause(String methodName) {
            return "@comWhereClauseProvider." + methodName;
        }
    }
}
