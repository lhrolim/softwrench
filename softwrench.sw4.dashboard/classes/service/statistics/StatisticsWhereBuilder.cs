using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Search;

namespace softwrench.sw4.dashboard.classes.service.statistics {


    public class StatisticsWhereBuilder : IWhereBuilder {

        private const string Default = " 1=1 ";

        private readonly DataConstraintsWhereBuilder _dataConstraintsWhereBuilder;

        public StatisticsWhereBuilder(DataConstraintsWhereBuilder dataConstraintsWhereBuilder) {
            _dataConstraintsWhereBuilder = dataConstraintsWhereBuilder;

        }

        public string BuildWhereClause(string entityName, SearchRequestDto searchDto = null) {
            return _dataConstraintsWhereBuilder.BuildWhereClause(entityName,null);
        }

        public IDictionary<string, object> GetParameters() {
            return _dataConstraintsWhereBuilder.GetParameters();
        }
    }
}
