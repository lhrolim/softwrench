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

namespace softWrench.sW4.Data.Persistence {


    public class MaximoHibernateDAO : BaseHibernateDAO {

        private static readonly ILog Log = LogManager.GetLogger(SwConstants.SQL_LOG);

        public MaximoHibernateDAO(IApplicationConfiguration applicationConfiguration)
            : base(applicationConfiguration) {
        }

        private static MaximoHibernateDAO _instance;
        public static MaximoHibernateDAO GetInstance() {
            if (_instance == null) {
                _instance =
                    SimpleInjectorGenericFactory.Instance.GetObject<MaximoHibernateDAO>(typeof(MaximoHibernateDAO));
            }
            return _instance;
        }

        public static ISession CurrentSession() {
            if (SWDBHibernateDAO.SessionManager.SessionFactory.IsClosed) {
                return SessionManager.Instance.OpenSession();
            }
            try {
                return SessionManager.Instance.CurrentSession;
            } catch (Exception) {
                return SessionManager.Instance.OpenSession();
            }
        }


        public class SessionManager : ISessionManager {
            private ISessionFactory _sessionFactory;

            public static ISessionFactory SessionFactory {
                get { return Instance._sessionFactory; }
            }

            private ISessionFactory GetSessionFactory() {
                return _sessionFactory;
            }

            public static SessionManager Instance {
                get {
                    return NestedSessionManager.SessionManager;
                }
            }

            public ISession OpenSession() {
                //nhibernate is always readonly
                var openSession = Instance.GetSessionFactory().OpenSession();
                openSession.FlushMode = FlushMode.Never;
                return openSession;
            }



            public ISession CurrentSession {
                get {
                    return Instance.GetSessionFactory().GetCurrentSession();
                }
            }

            public void Restart() {
                NestedSessionManager.SessionManager = new SessionManager();
            }

            private SessionManager() {
                var configuration = new NHibernate.Cfg.Configuration();
                //Create a dictionary to hold the properties
                IDictionary<string, string> properties = new Dictionary<string, string>();

                //Populate with some default properties
                properties.Add(NHibernate.Cfg.Environment.ConnectionDriver,
                    HibernateUtil.GetInstance().HibernateDriverName(DBType.Maximo));
                properties.Add(NHibernate.Cfg.Environment.Dialect,
                    HibernateUtil.GetInstance().HibernateDialect(DBType.Maximo));
                properties.Add(NHibernate.Cfg.Environment.ShowSql, "false");
                properties.Add(NHibernate.Cfg.Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider");
                //                      <property name="connection.provider">NHibernate.Connection.DriverConnectionProvider</property>
                //      <property name="proxyfactory.factory_class">NHibernate.Bytecode.DefaultProxyFactoryFactory, NHibernate</property>
                //      <property name="current_session_context_class">managed_web</property>

                //Add the connection and default schema
                properties[NHibernate.Cfg.Environment.ConnectionString] =
                    ApplicationConfiguration.DBConnectionString(DBType.Maximo);
                //                properties[NHibernate.Cfg.Environment.DefaultSchema] = defaultSchema;
                configuration.SetProperties(properties);

                _sessionFactory = configuration.BuildSessionFactory();
            }

            [UsedImplicitly]
            class NestedSessionManager {
                internal static SessionManager SessionManager = new SessionManager();
            }
        }


        protected override ILog GetLog() {
            return Log;
        }

        protected override ISessionManager GetSessionManager() {
            return SessionManager.Instance;
        }
    }
}
