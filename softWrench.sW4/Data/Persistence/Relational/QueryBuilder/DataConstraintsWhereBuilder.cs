using System;
using System.Collections.Generic;
using System.Linq;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector;
using softWrench.sW4.Util;
using softWrench.sW4.Data.Search;

namespace softWrench.sW4.Data.Persistence.Relational.QueryBuilder {

    class DataConstraintsWhereBuilder : IWhereBuilder, ISingletonComponent {
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
