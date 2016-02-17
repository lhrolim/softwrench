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


        protected override string GetDialect() {
            return HibernateUtil.HibernateDialect(DBType.Maximo);
        }

        protected override string GetDriverName() {
            return HibernateUtil.HibernateDriverName(DBType.Maximo);
        }

        protected override string GetConnectionString() {
            return ApplicationConfiguration.DBConnectionString(DBType.Maximo);
        }

        protected override ILog GetLog() {
            return Log;
        }


        protected override IEnumerable<Assembly> GetListOfAssemblies() {
            return null;
        }


    }
}
