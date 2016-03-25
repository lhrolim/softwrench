using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using softWrench.sW4.Data.Persistence;

namespace softwrench.sw4.dashboard.classes.service.statistics {
    public class StatisticsService : ISingletonComponent {

        private const string COUNT_BY_PROPERTY_ORDERED_TEMPLATE = @"select COALESCE({1}, 'NULL') as {1},count(*) as countBy 
                                                                        from {0} 
                                                                        group by {1} 
                                                                        order by " + COUNT_ORDER;
        private const string COUNT_ORDER = "countBy desc";

        private readonly IMaximoHibernateDAO _dao;

        public StatisticsService(IMaximoHibernateDAO dao) {
            _dao = dao;
        }

        /// <summary>
        /// Fetches the count of entries grouped by the property value ordered by the count descending.
        /// The result dictionary has the property values as keys an their respective count as the values. 
        /// </summary>
        /// <param name="entity">entity's name</param>
        /// <param name="property">entity's property</param>
        /// <param name="limit">number of entries in the result</param>
        /// <returns></returns>
        public async Task<IDictionary<string, int>> CountByProperty(string entity, string property, int limit = 0) {
            return await Task.Factory.StartNew(() => {
                var pagination = limit > 0 ? new PaginationData(limit, 1, COUNT_ORDER) : null;
                var formattedQuery = string.Format(COUNT_BY_PROPERTY_ORDERED_TEMPLATE, entity, property);

                return _dao.FindByNativeQuery(formattedQuery, new ExpandoObject(), pagination)
                            .Cast<IDictionary<string, object>>() // cast so ExpandoObject's properties can be indexed by string key
                            .Select(d => new KeyValuePair<string, int>((string)d[property], (int)d["countBy"]))
                            .ToDictionary(x => x.Key, x => x.Value);
            });
        }


    }
}
