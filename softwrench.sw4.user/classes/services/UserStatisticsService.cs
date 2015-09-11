using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using softwrench.sw4.user.classes.entities;

namespace softwrench.sw4.user.classes.services {
    public class UserStatisticsService : ISingletonComponent {

        private readonly ISWDBHibernateDAO _dao;

        public UserStatisticsService(ISWDBHibernateDAO dao) {
            _dao = dao;
        }

        public void UpdateStatistcsAsync(User user) {
            if (user.UserName.Equals("jobuser")) {
                return;
            }
            Task.Factory.StartNew(() => {

                var statistics = _dao.FindSingleByQuery<UserStatistics>(UserStatistics.ByUser, user);
                if (statistics == null) {
                    statistics = new UserStatistics {
                        User = user
                    };
                } else {
                    statistics.LoginCount++;
                    statistics.LastLoginDate = DateTime.Now;
                }
                _dao.Save(statistics);
            });
        }

        public UserStatistics LocateStatistics(User swUser) {
            return _dao.FindSingleByQuery<UserStatistics>(UserStatistics.ByUser, swUser);
        }
    }
}
