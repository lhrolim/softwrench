using System;
using System.Collections.Generic;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.SimpleInjector;

namespace softWrench.sW4.Preferences {

    public class GridFilterManager : ISingletonComponent {

        private readonly SWDBHibernateDAO _dao;

        public GridFilterManager(SWDBHibernateDAO dao) {
            _dao = dao;
        }

        public GridFilterAssociation CreateNewFilter(InMemoryUser user, String application, string queryString, string alias, string schema = "list") {
            var filter = new GridFilter {
                Alias = alias,
                Application = application,
                CreationDate = DateTime.Now,
                QueryString = queryString,
                Schema = schema
            };

            if (user.UserPreferences.ContainsFilter(filter)) {
                throw GridFilterException.FilterWithSameAliasAlreadyExists(alias, application);
            }

            var association = new GridFilterAssociation {
                Creator = true,
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
    }
}
