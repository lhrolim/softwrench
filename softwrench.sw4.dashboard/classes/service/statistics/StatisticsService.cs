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
        /// The result collection has the property values as FieldValue an their respective count as FieldCount.
        /// If a whereclause should be used AND the whereclause is a complete <code>SELECT</code> query it 
        /// will be used INSTEAD OF the regular statistics query and NOT AS A <code>WHERE</code> STATEMENT 
        /// on the regular statistics query.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<IEnumerable<StatisticsResponseEntry>> CountByProperty(StatisticsRequest request) {
            return await Task.Factory.StartNew(() => {
                var pagination = request.Limit > 0 ? new PaginationData(request.Limit, 1, COUNT_ORDER) : null;
                var query = BuildStatisticsQuery(request);

                return _dao.FindByNativeQuery(query, new ExpandoObject(), pagination)
                            .Cast<IDictionary<string, object>>() // cast so ExpandoObject's properties can be indexed by string key
                            .Select(d => {
                                var fieldValue = (string)d[request.Property];
                                var fieldCount = (int)d["countBy"];
                                // value is `null`: label configured by request. Otherwise try and grab the label from the query
                                var label = string.IsNullOrEmpty(fieldValue) || string.Equals(fieldValue, "NULL") 
                                            ? request.NullValueLabel 
                                            : d.ContainsKey("label") ? (string) d["label"] : null;

                                return new StatisticsResponseEntry(fieldValue: fieldValue, fieldCount: fieldCount, fieldLabel: label);
                            });
            });
        }

        private string BuildStatisticsQuery(StatisticsRequest request) {
            var whereClause = "";

            if (!string.IsNullOrEmpty(request.WhereClauseMetadataId)) {
                var whereClauseResult = _whereClauseFacade.Lookup(request.Application, new ApplicationLookupContext() {
                    MetadataId = request.WhereClauseMetadataId
                });
                // this warning here is a lie: (WhereClauseFacade implements IWhereClauseFacade).Lookup can return null
                if (whereClauseResult != null) {
                    whereClause = whereClauseResult.Query;
                }
            }

            // full query -> use it
            if (whereClause.TrimStart().StartsWith("select ", StringComparison.OrdinalIgnoreCase)) {
                return whereClause;
            }

            // format default query
            if (!string.IsNullOrEmpty(whereClause) && !whereClause.StartsWith("where", StringComparison.OrdinalIgnoreCase)) {
                whereClause = "where " + whereClause;
            }
            var formattedQuery = string.Format(COUNT_BY_PROPERTY_ORDERED_TEMPLATE, request.Entity, request.Property, whereClause);
            return formattedQuery;
        }

    }
}
