using System.Collections.Generic;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.SWDB;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Configuration.Services {

    public class ConditionService : ISingletonComponent {

        private SWDBHibernateDAO _dao;
        public ConditionService(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        [Transactional(DBType.Swdb)]
        public virtual IList<Condition> RemoveCondition(WhereClauseRegisterCondition condition, string currentCategoryKey) {
            var values = _dao.FindByQuery<PropertyValue>(PropertyValue.ByCondition, condition);
            var canDeleteCondition = !condition.Global || values.Count <= 1;
            foreach (var propertyValue in values) {
                if (propertyValue.Definition.FullKey.StartsWith(currentCategoryKey) || !condition.Global) {
                    _dao.Delete(propertyValue);
                }
            }
            if (canDeleteCondition) {
                _dao.Delete(condition.RealCondition);
            }
            return _dao.FindAll<Condition>(typeof(Condition));
        }
    }
}
