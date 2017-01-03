using cts.commons.persistence;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector;
using log4net;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace softWrench.sW4.Data.Persistence.SWDB {

    public class SWDBHibernateDAO : ReadWriteHibernateDAO, ISWDBHibernateDAO {

        private static readonly ILog Log = LogManager.GetLogger(SwConstants.SQLDB_LOG);

        private readonly HibernateUtil _hibernateUtil;


        public SWDBHibernateDAO(ApplicationConfigurationAdapter applicationConfiguration, HibernateUtil hibernateUtil) : base(applicationConfiguration, hibernateUtil) {
            _hibernateUtil = hibernateUtil;
        }

        public static SWDBHibernateDAO GetInstance() {
            return SimpleInjectorGenericFactory.Instance.GetObject<SWDBHibernateDAO>(typeof(SWDBHibernateDAO));
        }

        protected override int? GetCreatedByUser() {
            return SecurityFacade.CurrentUser().UserId;
        }


        public IList<T> FindByNativeQuery<T>(string queryst, PaginationData paginationData = null, params object[] parameters) where T : class {
            return RunTransactional((p) => {
                var query = BuildQuery(queryst, parameters, p.Session, true);
                if (paginationData != null) {
                    var pageSize = paginationData.PageSize;
                    query.SetMaxResults(pageSize);
                    query.SetFirstResult((paginationData.PageNumber - 1) * pageSize + 1);
                }
                return query.List<T>();
            });
        }



        protected override ILog GetLog() {
            return Log;
        }



      
        #region configuration

        protected override string GetDialect() {
            return HibernateUtil.HibernateDialect(DBType.Swdb);
        }

        protected override string GetDriverName() {
            return HibernateUtil.HibernateDriverName(DBType.Swdb);
        }

        protected override string GetConnectionString() {
            return ApplicationConfiguration.DBConnectionString(DBType.Swdb);
        }

        protected override bool IsMaximo() {
            return false;
        }

        protected override IEnumerable<Assembly> GetListOfAssemblies() {
            return AssemblyLocator.GetSWAssemblies();
        }

        #endregion


    }
}
