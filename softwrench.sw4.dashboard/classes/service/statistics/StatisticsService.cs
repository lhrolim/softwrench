using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using ctes = softwrench.sw4.dashboard.classes.service.statistics.StatisticsConstants;

namespace softwrench.sw4.dashboard.classes.service.statistics {
    public class StatisticsService : ISingletonComponent {

        private const string COUNT_BY_PROPERTY_ORDERED_TEMPLATE = "select COALESCE(CAST({1} as varchar), 'NULL') as {1}, count(*) as " + ctes.FIELD_VALUE_VARIABLE_NAME + 
                                                                    @" from {0} 
                                                                        {2}
                                                                        group by {1}
                                                                        order by " + COUNT_ORDER;

        private const string COUNT_ORDER = ctes.FIELD_VALUE_VARIABLE_NAME + " desc";

        private readonly IMaximoHibernateDAO _maxdao;
        private readonly ISWDBHibernateDAO _swdao;
        private readonly IWhereClauseFacade _whereClauseFacade;
        private readonly DataConstraintsWhereBuilder _whereBuilder;
        private readonly IContextLookuper _contextLookuper;

        public StatisticsService(IMaximoHibernateDAO maxdao, ISWDBHibernateDAO swdao,
            IWhereClauseFacade whereClauseFacade, DataConstraintsWhereBuilder whereBuilder, IContextLookuper contextLookuper) {
            _maxdao = maxdao;
            _swdao = swdao;
            _whereClauseFacade = whereClauseFacade;
            _whereBuilder = whereBuilder;
            _contextLookuper = contextLookuper;
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
            var pagination = request.Limit > 0 ? new PaginationData(request.Limit, 1, COUNT_ORDER) : null;
            var query = BuildStatisticsQuery(request);
            var dao = IsMaximoApplication(request.Application) ?  (IBaseHibernateDAO) _maxdao : _swdao;

            return await Task.Run(() => {
                var result = dao.FindByNativeQuery(query, new ExpandoObject(), pagination);
                return FormatQueryResult(result, request.Property, request.NullValueLabel);
            });
        }

        private static IEnumerable<StatisticsResponseEntry> FormatQueryResult(IEnumerable<dynamic> resultSet, string propertyName, string nullValueLabel) {
            return resultSet.Cast<IDictionary<string, object>>()
                // cast so ExpandoObject's properties can be indexed by string key
                .Select(d => {
                    var fieldValue = (string)d[propertyName];
                    var fieldCountLong = d[ctes.FIELD_VALUE_VARIABLE_NAME] as long?;
                    var fieldCount = fieldCountLong ?? Convert.ToInt64((int) d[ctes.FIELD_VALUE_VARIABLE_NAME]);
                    // value is `null`: label configured by request. Otherwise try and grab the label from the query
                    var label = string.IsNullOrEmpty(fieldValue) || string.Equals(fieldValue, "NULL")
                        ? nullValueLabel
                        : d.ContainsKey(ctes.FIELD_LABEL_VARIABLE_NAME) ? (string)d[ctes.FIELD_LABEL_VARIABLE_NAME] : null;

                    return new StatisticsResponseEntry(fieldValue: fieldValue, fieldCount: fieldCount, fieldLabel: label);
                });
        }

        public async Task<IEnumerable<StatisticsResponseEntry>> GetDataByAction(string action, StatisticsRequest request) {
            switch (action) {
                case "device_value":
                    return await DeviceValuesMonthly(request);
                default:
                    throw new InvalidOperationException(string.Format("No action that can provide statistical data for '{0}'", action));
            }
        }

        #region Specific API-unfriendly stuff

        private async Task<IEnumerable<StatisticsResponseEntry>> DeviceValuesMonthly(StatisticsRequest request) {
                var now = DateTime.Now.ToUniversalTime();
                var normalizedNow = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var months = new List<DateTime>(12);
                for (var i = 11; i > 0; i--) {
                    months.Add(normalizedNow.AddMonths(-i));    
                }
                months.Add(normalizedNow);

                var queryBuilder = new StringBuilder(@"
                    select d.rng as monthyear, max(d.value) as maxmonthvalue, min(d.value) as minmonthvalue
                        from 
	                        (
		                    select valuelong as value,
		                        case ");

                months.ForEach(m => {
                    var firstDayOfMonthTimestamp = m.ToUnixTimeStamp();
                    var lastDayOfMonth = new DateTime(m.Year, m.Month, DateTime.DaysInMonth(m.Year, m.Month), 23, 59, 59, DateTimeKind.Utc);
                    var lastDayOfMonthTimeStamp = lastDayOfMonth.ToUnixTimeStamp();
                    var monthName = m.ToString("MMMM", CultureInfo.InvariantCulture);
                    queryBuilder.AppendLine(string.Format("when timestamp between {0} and {1} then '{2}/{3}'", firstDayOfMonthTimestamp, lastDayOfMonthTimeStamp, monthName, m.Year));
                });

                queryBuilder.Append(@"
                        end as rng
		                from pesco_device_value
	                    ) d
                    where d.rng is not null
                    group by d.rng");

            return await Task.Run(() => {

                var results = _swdao.FindByNativeQuery(queryBuilder.ToString())
                                .Select(r => {
                                    var max = long.Parse(r["maxmonthvalue"]);
                                    var min = long.Parse(r["minmonthvalue"]);
                                    return new Dictionary<string, object> {
                                        { ctes.FIELD_VALUE_VARIABLE_NAME, max - min },
                                        { "monthyear", r["monthyear"] },
                                        { ctes.FIELD_LABEL_VARIABLE_NAME, r["monthyear"] }
                                    };
                                }).Where(r => ((long) r[ctes.FIELD_VALUE_VARIABLE_NAME]) > 0 );

                return FormatQueryResult(results, "monthyear", "NULL");
            });
        }

        #endregion

        #region Utils

        private static bool IsMaximoApplication(string app) {
            return MetadataProvider.FetchAvailableAppsAndEntities().Contains(app);
        }

        private string BuildStatisticsQuery(StatisticsRequest request) {
            _contextLookuper.FillGridContext(request.Application, SecurityFacade.CurrentUser());

            var contextWhereClause = _whereBuilder.BuildWhereClause(request.Entity, null);

            var requestWhereClause = "";

            if (!string.IsNullOrEmpty(request.WhereClauseMetadataId)) {
                var whereClauseResult = _whereClauseFacade.Lookup(request.Application, new ApplicationLookupContext() {
                    MetadataId = request.WhereClauseMetadataId
                });
                // this warning here is a lie: (WhereClauseFacade implements IWhereClauseFacade).Lookup can return null
                if (whereClauseResult != null) {
                    requestWhereClause = whereClauseResult.Query;
                }
            }

            var hasContextFilter = !string.IsNullOrEmpty(contextWhereClause) && !string.Equals(contextWhereClause, requestWhereClause);

            // full query -> use it
            if (requestWhereClause.TrimStart().StartsWith("select ", StringComparison.OrdinalIgnoreCase)) {
                return hasContextFilter 
                    ? requestWhereClause.Replace(ctes.CONTEXT_FILTER_VARIABLE_NAME, string.Format(" and ({0})", contextWhereClause)) 
                    : requestWhereClause;
            }

            // combine both whereclauses
            string fullWhereClause;
            var hasRequestFilter = !string.IsNullOrEmpty(requestWhereClause);
            if (!hasRequestFilter && !hasContextFilter) {
                fullWhereClause = "";
            } else if (!hasRequestFilter /*&& hasContextFilter*/) {
                fullWhereClause = contextWhereClause;
            } else if (/*hasRequestFilter &&*/ !hasContextFilter) {
                fullWhereClause = requestWhereClause;
            } else {
                fullWhereClause = requestWhereClause + string.Format(" and ({0})", contextWhereClause);
            }
            if (!string.IsNullOrEmpty(fullWhereClause) && !fullWhereClause.StartsWith("where", StringComparison.OrdinalIgnoreCase)) {
                fullWhereClause = "where " + fullWhereClause;
            }

            // format default query
            var formattedQuery = string.Format(COUNT_BY_PROPERTY_ORDERED_TEMPLATE, request.Entity, request.Property, fullWhereClause);
            return formattedQuery;
        }

        #endregion

    }
}
