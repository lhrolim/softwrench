using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector;
using NHibernate.Util;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Util;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {

    public class DataConstraintsWhereBuilder : IWhereBuilder, ISingletonComponent {
        private const string Default = " 1=1 ";

        private readonly IWhereClauseFacade _whereClauseFacade;

        public DataConstraintsWhereBuilder(IWhereClauseFacade whereClauseFacade) {
            _whereClauseFacade = whereClauseFacade;
        }

        public string BuildWhereClause(string entityName, SearchRequestDto searchDto) {
            if (searchDto != null && searchDto.IgnoreWhereClause) {
                return Default;
            }
            var nameToLookup = GetNameToLookup(entityName, searchDto);
            if (nameToLookup == null) {
                return Default;
            }
            var context = searchDto == null ? null : searchDto.Context;

            var whereClauseResult = _whereClauseFacade.Lookup(nameToLookup, context);
            return whereClauseResult == null || whereClauseResult.IsEmpty()
                    ? Default
                    : whereClauseResult.Query;
        }

        //TODO: move to whereclause facade


        private string GetNameToLookup(string entityName, SearchRequestDto searchDto) {
            var applications = MetadataProvider.Applications().ToList().Where(a => a.Entity.EqualsIc(entityName)).ToList();
            if (!applications.Any()) {
                return MetadataProvider.Entity(entityName).Name;
            }
            if (applications.Count() == 1) {
                return applications[0].ApplicationName;
            }

            var appName = searchDto != null && searchDto.Key != null ? searchDto.Key.ApplicationName : null;
            if (appName == null) {
                return applications[0].ApplicationName;
            }

            var matchedApplication = applications.Where(a => appName.Equals(a.ApplicationName)).ToList();
            return matchedApplication.Any() ? matchedApplication[0].ApplicationName : applications[0].ApplicationName;
        }

        public IDictionary<string, object> GetParameters() {
            return null;
        }

    }
}
