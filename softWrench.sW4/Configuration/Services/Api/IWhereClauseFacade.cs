using System;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Configuration.Services.Api {
    public interface IWhereClauseFacade : ISingletonComponent {

        WhereClauseResult Lookup(string applicationName, ApplicationLookupContext lookupContext = null);

        void Register(string applicationName, String query, WhereClauseRegisterCondition condition = null,bool validate=true);


    }
}