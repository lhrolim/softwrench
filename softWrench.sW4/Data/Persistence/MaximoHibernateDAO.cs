using cts.commons.persistence;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using JetBrains.Annotations;
using log4net;
using NHibernate;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace softWrench.sW4.Data.Persistence {


    public class MaximoHibernateDAO : BaseHibernateDAO, IMaximoHibernateDAO {

        private static readonly ILog Log = LogManager.GetLogger(SwConstants.SQL_LOG);

        public MaximoHibernateDAO(ApplicationConfigurationAdapter applicationConfiguration, HibernateUtil hibernateUtil) : base(applicationConfiguration, hibernateUtil) {
        }

        private static MaximoHibernateDAO _instance;

        public static MaximoHibernateDAO GetInstance() {
            if (_instance == null) {
                _instance =
                    SimpleInjectorGenericFactory.Instance.GetObject<MaximoHibernateDAO>(typeof(MaximoHibernateDAO));
            }
            return _instance;
        }

        /// <summary>
        /// Use this method only for exceptional scenarios, as we´re not intended to update Maximo straight to the database
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public void ExecuteSql(string sql, params object[] parameters) {
            using (var session = GetSession()) {
                using (var transaction = session.BeginTransaction()) {
                    var query = session.CreateSQLQuery(sql);
                    if (parameters != null) {
                        for (int i = 0; i < parameters.Length; i++) {
                            query.SetParameter(i, parameters[i]);
                        }
                    }
                    query.ExecuteUpdate();
                    transaction.Commit();
                }
            }
        }


        protected override string GetDialect() {
            return HibernateUtil.HibernateDialect(DBType.Maximo);
        }

        protected override string GetDriverName() {
            return HibernateUtil.HibernateDriverName(DBType.Maximo);
        }

        protected override string GetConnectionString() {
            return ApplicationConfiguration.DBConnectionString(DBType.Maximo);
        }

        protected override bool IsMaximo() {
            return true;
        }

        protected override ILog GetLog() {
            return Log;
        }


        protected override IEnumerable<Assembly> GetListOfAssemblies() {
            return null;
        }


    }
}
