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

        public static object CurrentSession() {
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
            private readonly ISessionFactory _sessionFactory;

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
                var openSession = Instance.GetSessionFactory().OpenStatelessSession();
//                openSession.FlushMode = FlushMode.Never;
                return new SessionAdapter(openSession);
            }

            public ISession CurrentSession {
                get {
                    return Instance.GetSessionFactory().GetCurrentSession();
                }
            }

            private SessionManager() {
                var configuration = new NHibernate.Cfg.Configuration();
                //Create a dictionary to hold the properties
                IDictionary<string, string> properties = new Dictionary<string, string>();

                //Populate with some default properties
                properties.Add(NHibernate.Cfg.Environment.ConnectionDriver, HibernateUtil.HibernateDriverName(ApplicationConfiguration.DBType.Maximo));
                properties.Add(NHibernate.Cfg.Environment.Dialect, HibernateUtil.HibernateDialect(ApplicationConfiguration.DBType.Maximo));
                properties.Add(NHibernate.Cfg.Environment.ShowSql, "false");
                properties.Add(NHibernate.Cfg.Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider");
                //                      <property name="connection.provider">NHibernate.Connection.DriverConnectionProvider</property>
                //      <property name="proxyfactory.factory_class">NHibernate.Bytecode.DefaultProxyFactoryFactory, NHibernate</property>
                //      <property name="current_session_context_class">managed_web</property>

                //Add the connection and default schema
                properties[NHibernate.Cfg.Environment.ConnectionString] =
                    ApplicationConfiguration.DBConnectionString(ApplicationConfiguration.DBType.Maximo);
                //                properties[NHibernate.Cfg.Environment.DefaultSchema] = defaultSchema;
                configuration.SetProperties(properties);

                _sessionFactory = configuration.BuildSessionFactory();
            }

            [UsedImplicitly]
            class NestedSessionManager {
                internal static readonly SessionManager SessionManager = new SessionManager();
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
