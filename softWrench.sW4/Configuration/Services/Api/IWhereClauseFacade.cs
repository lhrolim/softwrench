using System;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Configuration.Services.Api {
    public interface IWhereClauseFacade : ISingletonComponent {

        //TODO: refactor it so that the evaluation is done inside of the facade and the return is already a string
        WhereClauseResult Lookup(string applicationName, ApplicationLookupContext lookupContext = null, ContextHolder contextHolder = null);

        void Register(string applicationName, String query, WhereClauseRegisterCondition condition = null,bool validate=true);


    }
}