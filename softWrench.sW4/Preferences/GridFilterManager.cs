﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.SqlCommand;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Preferences {

    public class GridFilterManager : ISingletonComponent {

        private readonly SWDBHibernateDAO _dao;

        public GridFilterManager(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        public GridFilterAssociation CreateNewFilter(InMemoryUser user, String application, string fields, string operators, string values, string alias, string schema = "list") {
            var filter = new GridFilter {
                Alias = alias,
                Application = application,
                CreationDate = DateTime.Now,
                Fields = fields,
                Operators = operators,
                Values = values,
                Schema = schema,
                Creator = user.DBUser
            };

            if (user.UserPreferences.ContainsFilter(filter, user)) {
                throw GridFilterException.FilterWithSameAliasAlreadyExists(alias, application);
            }

            var association = new GridFilterAssociation {
                Filter = filter,
                User = user.DBUser,
                JoiningDate = DateTime.Now
            };
            association = _dao.Save(association);
            user.UserPreferences.GridFilters.Add(association);
            return association;
        }

        public ISet<GridFilterAssociation> LoadAllOfUser(int? userId) {
            return new HashSet<GridFilterAssociation>(_dao.FindByQuery<GridFilterAssociation>(GridFilterAssociation.ByUserId, userId));
        }

        public GridFilter UpdateFilter(InMemoryUser user, string fields, string alias, string operators, string values, int? id) {
            var filter = _dao.FindByPK<GridFilter>(typeof(GridFilter), id);
            if (filter == null) {
                throw GridFilterException.FilterNotFound(id);
            }
            filter.Operators = operators;
            filter.Fields = fields;
            filter.Values = values;
            filter.Alias = alias;
            filter.UpdateDate = DateTime.Now;
            var updateFilter = _dao.Save(filter);
            var memoryAssociation = user.UserPreferences.GridFilters.FirstOrDefault(a => a.Filter.Id == id);
            if (memoryAssociation != null) {
                //update the filter of the user, no logout required
                memoryAssociation.Filter = updateFilter;
            }
            return updateFilter;
        }

        public GridFilterAssociation DeleteFilter(InMemoryUser currentUser, int? id, int? creatorId) {

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
            currentUser.UserPreferences.GridFilters.Remove(association);
            return association;

        }
    }
}
