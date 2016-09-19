using System.Collections.Generic;
using System.Threading.Tasks;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Security.Context;
using cts.commons.simpleinjector;
using JetBrains.Annotations;
using softwrench.sw4.user.classes.entities;
using softWrench.sW4.Metadata.Security;

namespace softWrench.sW4.Configuration.Services.Api {
    public interface IWhereClauseFacade : ISingletonComponent {

        //TODO: refactor it so that the evaluation is done inside of the facade and the return is already a string
        [NotNull]
        Task<WhereClauseResult> LookupAsync([NotNull]string applicationName, ApplicationLookupContext lookupContext = null, ContextHolder contextHolder = null);

        WhereClauseResult Lookup([NotNull]string applicationName, ApplicationLookupContext lookupContext = null, ContextHolder contextHolder = null);

        Task RegisterAsync([NotNull]string applicationName, [NotNull]string query, WhereClauseRegisterCondition condition = null,bool validate=false);

        void Register([NotNull]string applicationName, [NotNull]string query, WhereClauseRegisterCondition condition = null, bool validate = false);

        /// <summary>
        /// Given an application returns a list of associated profiles that have different whereclauses on it, constrained to the ones that the current user have
        /// 
        /// This is part of https://controltechnologysolutions.atlassian.net/browse/SWWEB-1780 implementation.
        /// 
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="loggedUser">The user to restrict the profiles</param>
        /// <returns>Collection of profile ids</returns>
        [NotNull]
        Task<ISet<UserProfile>> ProfilesByApplication([NotNull]string applicationName, [NotNull] InMemoryUser loggedUser);

    }
}