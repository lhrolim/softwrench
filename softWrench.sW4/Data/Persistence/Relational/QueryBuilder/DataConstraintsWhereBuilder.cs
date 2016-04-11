using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Util;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Security;

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
            var nameToLookup = GetNameToLookup(entityName);
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
        

        private static string GetNameToLookup(string entityName) {
            var application = MetadataProvider.Applications().FirstOrDefault(a => a.Entity == entityName);
            if (application == null) {
                return MetadataProvider.Entity(entityName).Name;
            }
            return application.ApplicationName;
        }

        public IDictionary<string, object> GetParameters() {
            return null;
        }

    }
}
