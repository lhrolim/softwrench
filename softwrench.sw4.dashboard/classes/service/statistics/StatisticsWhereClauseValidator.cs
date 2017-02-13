using System;
using System.Dynamic;
using cts.commons.persistence;
using softwrench.sw4.dashboard.classes.startup;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Metadata;
using ctes = softwrench.sw4.dashboard.classes.service.statistics.StatisticsConstants;

namespace softwrench.sw4.dashboard.classes.service.statistics {
    public class StatisticsWhereClauseValidator : IWhereClauseValidator {

        private const string CountOrder = ctes.FIELD_VALUE_VARIABLE_NAME + " desc";

        private readonly IMaximoHibernateDAO _maxdao;
        private readonly ISWDBHibernateDAO _swdao;

        public StatisticsWhereClauseValidator(IMaximoHibernateDAO maxdao, ISWDBHibernateDAO swdao) {
            _maxdao = maxdao;
            _swdao = swdao;
        }

        public bool DoesValidate(string applicationName, WhereClauseCondition conditionToValidateAgainst = null) {
            if (conditionToValidateAgainst?.AppContext?.MetadataId == null) {
                return false;
            }

            var metadataId = conditionToValidateAgainst.AppContext.MetadataId;
            // only uses custom validation for statistics queries that have custom base queries
            return StatisticsQueryProvider.BaseStatisticsSelectQueries.ContainsKey(metadataId);
        }

        public void Validate(string applicationName, string whereClause, WhereClauseCondition conditionToValidateAgainst = null) {
            if (conditionToValidateAgainst?.AppContext?.MetadataId == null) {
                throw new Exception("Dashboard widget metadata id not found.");
            }
            var metadataId = conditionToValidateAgainst.AppContext.MetadataId;
            string fixedSelectClause;
            StatisticsQueryProvider.BaseStatisticsSelectQueries.TryGetValue(metadataId, out fixedSelectClause);

            if (string.IsNullOrEmpty(fixedSelectClause)) {
                throw new Exception("Dashboard widget fixed select clause not found.");
            }

            var query = fixedSelectClause.Replace(ctes.CONTEXT_FILTER_VARIABLE_NAME, $" where {whereClause} ");
            var dao = MetadataProvider.FetchAvailableAppsAndEntities().Contains(applicationName) ? (IBaseHibernateDAO)_maxdao : _swdao;
            dao.FindByNativeQuery(query, new ExpandoObject(), new PaginationData(1, 1, CountOrder));
        }
    }
}
