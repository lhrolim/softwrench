using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {

    class DataConstraintsWhereBuilder : IWhereBuilder, ISingletonComponent {
        private const string Default = " 1=1 ";

        private readonly IWhereClauseFacade _whereClauseFacade;

        public DataConstraintsWhereBuilder(IWhereClauseFacade whereClauseFacade) {
            _whereClauseFacade = whereClauseFacade;

        }

        public string BuildWhereClause(string entityName, QueryCacheKey.QueryMode queryMode, SearchRequestDto searchDto = null) {
            var baseWhereClause = DoBuildBaseWhereClause(entityName, searchDto);
            if (queryMode.Equals(QueryCacheKey.QueryMode.Detail)) {
                return ApplySpecificDetailClauses(entityName, baseWhereClause);
            }
            return baseWhereClause;
        }

        private string ApplySpecificDetailClauses(string entity, string baseWhereClause) {
            var user = SecurityFacade.CurrentUser();
            if (user.IsInRole("tom") || user.IsInRole("itom") && entity.EqualsAny("servicerequest", "asset", "incident")) {
                return Default;
            }
            if (IsWWUser(user)) {
                return Default;
            }

            return baseWhereClause;
        }


        public static bool IsWWUser(InMemoryUser user) {
            return user.PersonGroups.Any(p => p.PersonGroup.Name.Equals("C-HLC-WW-AR-WW"));
        }

        private string DoBuildBaseWhereClause(string entityName, SearchRequestDto searchDto) {
            if (searchDto != null && searchDto.IgnoreWhereClause) {
                return Default;
            }
            var nameToLookup = GetNameToLookup(entityName);
            if (nameToLookup == null) {
                return Default;
            }
            var context = searchDto == null ? null : searchDto.Context;


            var whereClauseResult = _whereClauseFacade.Lookup(nameToLookup, context);


            if (whereClauseResult == null || whereClauseResult.IsEmpty()) {
                return Default;
            }
            var user = SecurityFacade.CurrentUser();
            if (!String.IsNullOrEmpty(whereClauseResult.Query)) {
                return DefaultValuesBuilder.ConvertAllValues(whereClauseResult.Query, user);
            }
            if (!String.IsNullOrEmpty(whereClauseResult.ServiceName)) {
                var ob = SimpleInjectorGenericFactory.Instance.GetObject<object>(whereClauseResult.ServiceName);
                if (ob != null) {
                    var result = ReflectionUtil.Invoke(ob, whereClauseResult.MethodName, new object[] { });
                    if (!(result is String)) {
                        return Default;
                    }
                    return DefaultValuesBuilder.ConvertAllValues((string)result, user);
                }
            }

            return Default;
        }

        private static String GetNameToLookup(string entityName) {
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
