using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Security.Context;

namespace softwrench.sw4.dashboard.classes.service.statistics {
    public class StatisticsService : ISingletonComponent {

        private const string COUNT_BY_PROPERTY_ORDERED_TEMPLATE = @"select COALESCE(CAST({1} as varchar), 'NULL') as {1},count(*) as countBy 
                                                                        from {0} 
                                                                        {2}
                                                                        group by {1}
                                                                        order by " + COUNT_ORDER;
        private const string COUNT_ORDER = "countBy desc";

        private readonly IMaximoHibernateDAO _dao;
        private readonly IWhereClauseFacade _whereClauseFacade;

        public StatisticsService(IMaximoHibernateDAO dao, IWhereClauseFacade whereClauseFacade) {
            _dao = dao;
            _whereClauseFacade = whereClauseFacade;
        }

        /// <summary>
        /// Fetches the count of entries grouped by the property value ordered by the count descending.
        /// The result dictionary has the property values as keys an their respective count as the values. 
        /// </summary>
        /// <param name="entity">entity's name</param>
        /// <param name="property">entity's property</param>
        /// <param name="whereClauseMetadataId">whereclause's MetadataId of the whereclause to be applied. Defaults to null</param>
        /// <param name="limit">number of entries in the result</param>
        /// <returns></returns>
        public async Task<IDictionary<string, int>> CountByProperty(string entity, string property, string whereClauseMetadataId = null, int limit = 0) {
            return await Task.Factory.StartNew(() => {
                var pagination = limit > 0 ? new PaginationData(limit, 1, COUNT_ORDER) : null;
                var whereClause = WhereClauseQuery(entity, whereClauseMetadataId);

                var formattedQuery = string.Format(COUNT_BY_PROPERTY_ORDERED_TEMPLATE, entity, property, whereClause);

                return _dao.FindByNativeQuery(formattedQuery, new ExpandoObject(), pagination)
                            .Cast<IDictionary<string, object>>() // cast so ExpandoObject's properties can be indexed by string key
                            .Select(d => new KeyValuePair<string, int>((string)d[property], (int)d["countBy"]))
                            .ToDictionary(x => x.Key, x => x.Value);
            });
        }

        private string WhereClauseQuery(string entity, string whereClauseMetadataId) {
            if (string.IsNullOrEmpty(whereClauseMetadataId)) return "";

            var whereClauseResult = _whereClauseFacade.Lookup(entity, new ApplicationLookupContext() {
                MetadataId = whereClauseMetadataId
            });

            if (whereClauseResult == null) return "";

            var whereClause = whereClauseResult.Query;
            if (string.IsNullOrEmpty(whereClause)) return "";

            if (!whereClause.StartsWith("where", StringComparison.OrdinalIgnoreCase)) {
                whereClause = "where " + whereClause;
            }

            return whereClause;
        }


    }
}
