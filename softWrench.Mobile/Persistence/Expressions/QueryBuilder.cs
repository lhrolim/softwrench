using System.Collections.Generic;
using System.Linq;
using System.Text;
using softWrench.Mobile.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;

namespace softWrench.Mobile.Persistence.Expressions
{
    internal static class QueryBuilder
    {
        // TODO: review this limit.
        private const int MaxRows = 300;

        public static Query Build(ApplicationSchemaDefinition application, IEnumerable<FilterExpression> filters)
        {
            var filtersAsList = filters as IList<FilterExpression> ?? filters.ToList();
            var parameters = new List<object>(filtersAsList.Count + 1);
            var sql = new StringBuilder(filtersAsList.Count * 64);

            sql.Append("select * from DataMap where application = ? ");
            parameters.Add(application.ApplicationName);

            if (filtersAsList.Any())
            {
                sql.Append("and (");

                for (var i = 0; i < filtersAsList.Count; i++)
                {
                    var filter = filtersAsList[i];
                    var suffix = (i < filtersAsList.Count - 1) ? "and " : ") ";

                    sql.AppendFormat("{0} {1}", filter.BuildSql(), suffix);
                    parameters.AddRange(filter.BuildParameters());
                }
            }

            sql.Append(string.Format("limit {0}", MaxRows));
            return new Query(sql.ToString(), parameters.ToArray());
        }

        internal class Query
        {
            private readonly string _sql;
            private readonly object[] _parameters;

            public Query(string sql, object[] parameters)
            {
                _sql = sql;
                _parameters = parameters;
            }

            public string Sql
            {
                get { return _sql; }
            }

            public object[] Parameters
            {
                get { return _parameters; }
            }
        }
    }
}

