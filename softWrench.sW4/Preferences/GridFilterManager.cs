using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.persistence.Transaction;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Security;
using cts.commons.simpleinjector;

namespace softWrench.sW4.Preferences {

    public class GridFilterManager : ISingletonComponent {

        private readonly SWDBHibernateDAO _dao;

        public GridFilterManager(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        [Transactional(DBType.Swdb)]
        public virtual GridFilterAssociation CreateNewFilter(InMemoryUser user, string application, string fields, string operators, string values,string template, string alias, string advancedSearch, string schema = "list") {
            var filter = new GridFilter {
                Alias = alias,
                Application = application,
                CreationDate = DateTime.Now,
                Fields = fields,
                Operators = operators,
                Values = values,
                Schema = schema,
                Creator = user.DBUser,
                Template = template,
                AdvancedSearch = advancedSearch
            };

            if (user.GridPreferences.ContainsFilter(filter, user)) {
                throw GridFilterException.FilterWithSameAliasAlreadyExists(alias, application);
            }

            var association = new GridFilterAssociation {
                Filter = filter,
                User = user.DBUser,
                JoiningDate = DateTime.Now
            };
            association = _dao.Save(association);
            user.GridPreferences.GridFilters.Add(association);
            return association;
        }

        public ISet<GridFilterAssociation> LoadAllOfUser(int? userId) {
            return new HashSet<GridFilterAssociation>(_dao.FindByQuery<GridFilterAssociation>(GridFilterAssociation.ByUserId, userId));
        }

        [Transactional(DBType.Swdb)]
        public virtual GridFilter UpdateFilter(InMemoryUser user, string fields, string alias, string operators, string values, string template, string advancedSearch, int? id) {
            var filter = _dao.FindByPK<GridFilter>(typeof(GridFilter), id);
            if (filter == null) {
                throw GridFilterException.FilterNotFound(id);
            }
            filter.Operators = operators;
            filter.Fields = fields;
            filter.Values = values;
            filter.Alias = alias;
            filter.Template = template;
            filter.AdvancedSearch = advancedSearch;
            filter.UpdateDate = DateTime.Now;
            var updateFilter = _dao.Save(filter);
            var memoryAssociation = user.GridPreferences.GridFilters.FirstOrDefault(a => a.Filter.Id == id);
            if (memoryAssociation != null) {
                //update the filter of the user, no logout required
                memoryAssociation.Filter = updateFilter;
            }
            return updateFilter;
        }

        [Transactional(DBType.Swdb)]
        public virtual GridFilterAssociation DeleteFilter(InMemoryUser currentUser, int? id, int? creatorId) {

            var association = _dao.FindSingleByQuery<GridFilterAssociation>(GridFilterAssociation.ByUserIdAndFilter, currentUser.DBId, id);
            _dao.Delete(association);
            if (currentUser.DBId == creatorId) {
                //lets see if it´s safe to delete the filter it self or just the association
                var count = _dao.FindSingleByQuery<long>(GridFilterAssociation.CountByFilter, id);
                if (count == 0) {
                    _dao.Delete(association.Filter);
                }

            }
            //remove it from memory as well
            currentUser.GridPreferences.GridFilters.Remove(association);
            return association;

        }
    }
}
