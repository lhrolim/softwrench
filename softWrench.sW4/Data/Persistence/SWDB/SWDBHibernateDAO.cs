using cts.commons.persistence;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using JetBrains.Annotations;
using log4net;
using NHibernate;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Interfaces;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace softWrench.sW4.Data.Persistence.SWDB {

    public class SWDBHibernateDAO : BaseHibernateDAO {

        private static readonly ILog Log = LogManager.GetLogger(SwConstants.SQLDB_LOG);

        public SWDBHibernateDAO(IApplicationConfiguration applicationConfiguration)
            : base(applicationConfiguration) {
        }

        private static SWDBHibernateDAO _instance;
        public static SWDBHibernateDAO GetInstance() {
            if (_instance == null) {
                _instance =
                    SimpleInjectorGenericFactory.Instance.GetObject<SWDBHibernateDAO>(typeof(SWDBHibernateDAO));
            }
            return _instance;
        }

        public T Save<T>(T ob) where T : class {
            using (var session = SessionManager.Instance.OpenSession()) {
                using (var transaction = session.BeginTransaction()) {
                    var b = ob as IBaseEntity;
                    if (b != null && (b.Id == 0 || b.Id == null)) {
                        b.Id = (int)session.Save(ob);
                    } else {
                        ob = session.Merge(ob);
                    }
                    transaction.Commit();
                    return ob;
                }
            }
        }

        public void DeleteCollection(IEnumerable<object> collection) {
            using (var session = GetSessionManager().OpenSession()) {
                using (var transaction = session.BeginTransaction()) {
                    if (collection != null) {
                        foreach (var element in collection) {
                            session.Delete(element);
                        }
                        transaction.Commit();
                    }
                }
            }
        }

        public void Delete(Object ob) {
            using (var session = SessionManager.Instance.OpenSession()) {
                using (var transaction = session.BeginTransaction()) {
                    session.Delete(ob);
                    transaction.Commit();
                }
            }
        }

        public T FindByPK<T>(Type type, object id, params string[] toEager) {
            using (var session = SessionManager.Instance.OpenSession()) {
                using (session.BeginTransaction()) {
                    var ob = session.Load(type, id);
                    for (int i = 0; i < toEager.Length; i++) {
                        object property = ReflectionUtil.GetProperty(ob, toEager[i]);
                        NHibernateUtil.Initialize(property);
                    }
                    return (T)ob;
                }
            }
        }


        public T FindSingleByQuery<T>(String queryst, params object[] parameters) {
            using (var session = SessionManager.Instance.OpenSession()) {
                using (session.BeginTransaction()) {
                    var query = BuildQuery(queryst, parameters, session);
                    return (T)query.UniqueResult();
                }
            }
        }


        public IList<T> FindByQuery<T>(String queryst, params object[] parameters) where T : class {
            using (var session = SessionManager.Instance.OpenSession()) {
                using (session.BeginTransaction()) {
                    var query = BuildQuery(queryst, parameters, session);
                    return query.List<T>();
                }
            }
        }


        public IList<T> FindByNativeQuery<T>(String queryst, PaginationData paginationData = null, params object[] parameters) where T : class {
            using (var session = SessionManager.Instance.OpenSession()) {
                using (session.BeginTransaction()) {
                    var query = BuildQuery(queryst, parameters, session, true);
                    if (paginationData != null) {
                        var pageSize = paginationData.PageSize;
                        query.SetMaxResults(pageSize);
                        query.SetFirstResult((paginationData.PageNumber - 1) * pageSize + 1);
                    }
                    return query.List<T>();
                }
            }
        }

        protected override ILog GetLog() {
            return Log;
        }

        protected override ISessionManager GetSessionManager() {
            return SessionManager.Instance;
        }

        public IList<T> FindAll<T>(Type type) where T : class {
            using (var session = SessionManager.Instance.OpenSession()) {
                using (session.BeginTransaction()) {
                    var query = BuildQuery("from " + type.Name, (object[])null, session);
                    return query.List<T>();
                }
            }
        }

        public void ExecuteSql(string sql, params object[] parameters) {
            using (var session = SessionManager.Instance.OpenSession()) {
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

        public static ISession CurrentSession() {
            if (SessionManager.SessionFactory.IsClosed) {
                return SessionManager.Instance.OpenSession();
            }
            try {
                var currentSession = SessionManager.Instance.CurrentSession;
                if (!currentSession.IsOpen) {
                    return SessionManager.Instance.OpenSession();
                }
                return currentSession;
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
                return Instance.GetSessionFactory().OpenSession();
            }

            public void Restart() {
                NestedSessionManager.SessionManager = new SessionManager();
            }

            public ISession CurrentSession {
                get {
                    return Instance.GetSessionFactory().GetCurrentSession();
                }
            }

            private SessionManager() {
                var configuration = new NHibernate.Cfg.Configuration();
                configuration.AddAssembly(Assembly.GetCallingAssembly());
                IDictionary<string, string> properties = new Dictionary<string, string>();
                properties[NHibernate.Cfg.Environment.ConnectionString] =
                    ApplicationConfiguration.DBConnectionString(DBType.Swdb);
                properties.Add(NHibernate.Cfg.Environment.ConnectionDriver,
                    HibernateUtil.GetInstance().HibernateDriverName(DBType.Swdb));
                properties.Add(NHibernate.Cfg.Environment.Dialect,
                    HibernateUtil.GetInstance().HibernateDialect(DBType.Swdb));
                properties.Add(NHibernate.Cfg.Environment.ShowSql, "false");
                properties.Add(NHibernate.Cfg.Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider");
                properties.Add(NHibernate.Cfg.Environment.ProxyFactoryFactoryClass,
                    "NHibernate.Bytecode.DefaultProxyFactoryFactory, NHibernate");
                properties.Add(NHibernate.Cfg.Environment.CurrentSessionContextClass, "managed_web");
                configuration.SetProperties(properties);

                var findTypesAnnotattedWith = AttributeUtil.FindTypesAnnotattedWith(typeof(ClassAttribute),
                    typeof(JoinedSubclassAttribute));
                foreach (var nHibernateType in findTypesAnnotattedWith) {
                    configuration.AddInputStream(HbmSerializer.Default.Serialize(nHibernateType));
                }


                _sessionFactory = configuration.BuildSessionFactory();
            }

            [UsedImplicitly]
            class NestedSessionManager {
                internal static SessionManager SessionManager =
                    new SessionManager();
            }
        }





    }
}
