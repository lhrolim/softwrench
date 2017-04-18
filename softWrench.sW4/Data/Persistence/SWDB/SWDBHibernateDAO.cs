using JetBrains.Annotations;
using log4net;
using NHibernate;
using softWrench.sW4.Audit;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Data.Entities.Historical;
using softWrench.sW4.Security.Entities;
using softWrench.sW4.Security.Interfaces;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace softWrench.sW4.Data.Persistence.SWDB {

    public class SWDBHibernateDAO : BaseHibernateDAO {

        private static readonly ILog Log = LogManager.GetLogger(SwConstants.SQLDB_LOG);

        public T Save<T>(T ob) where T : class {
            using (var session = SessionManager.Instance.OpenSession()) {
                using (var transaction = session.BeginTransaction()) {
                    ob =DoSave(ob, session);
                    transaction.Commit();
                    return ob;
                }
            }
        }

        private static T DoSave<T>(T ob, ISession session) where T : class {
            var b = ob as IBaseEntity;
            if (b != null && (b.Id == 0 || b.Id == null)) {
                b.Id = (int)session.Save(ob);
            } else {
                ob = session.Merge(ob);
            }
            
            return ob;
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


        public T FindSingleByQuery<T>(String queryst, params object[] parameters) where T : class {
            using (var session = SessionManager.Instance.OpenSession()) {
                using (session.BeginTransaction()) {
                    var query = BuildQuery(queryst, parameters, session);
                    return (T)query.UniqueResult();
                }
            }
        }

        public ICollection<T> BulkSave<T>(IEnumerable<T> items) where T : class {
            if (items == null || !items.Any()) {
                return items.ToList<T>();
            }
            var result = new List<T>(items.Count<T>());


            using (var session = SessionManager.Instance.OpenSession()) {
                using (var transaction = session.BeginTransaction()) {
                    foreach (var item in items) {
                        result.Add(DoSave(item,session));
                    }
                    transaction.Commit();
                    return result;
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
                    var result = query.ExecuteUpdate();
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
                return Instance.GetSessionFactory().OpenSession();
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
                properties[NHibernate.Cfg.Environment.ConnectionString] = ApplicationConfiguration.DBConnectionString(ApplicationConfiguration.DBType.Swdb);
                properties.Add(NHibernate.Cfg.Environment.ConnectionDriver, HibernateUtil.HibernateDriverName(ApplicationConfiguration.DBType.Swdb));
                properties.Add(NHibernate.Cfg.Environment.Dialect, HibernateUtil.HibernateDialect(ApplicationConfiguration.DBType.Swdb));
                properties.Add(NHibernate.Cfg.Environment.ShowSql, "false");
                properties.Add(NHibernate.Cfg.Environment.ConnectionProvider, "NHibernate.Connection.DriverConnectionProvider");
                properties.Add(NHibernate.Cfg.Environment.ProxyFactoryFactoryClass, "NHibernate.Bytecode.DefaultProxyFactoryFactory, NHibernate");
                properties.Add(NHibernate.Cfg.Environment.CurrentSessionContextClass, "managed_web");



                configuration.SetProperties(properties);
                //TODO: make this modular
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(User)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(Role)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(RoleGroup)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(DataConstraint)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(UserProfile)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(UserCustomConstraint)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(UserCustomRole)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(Category)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(PropertyDefinition)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(PropertyValue)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(Condition)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(WhereClauseCondition)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(PersonGroup)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(PersonGroupAssociation)));

                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(AuditTrail)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(ISMAuditTrail)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(ExtraAttributes)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(HistWorkorder)));
                configuration.AddInputStream(NHibernate.Mapping.Attributes.HbmSerializer.Default.Serialize(typeof(HistTicket)));

                _sessionFactory = configuration.BuildSessionFactory();
            }

            [UsedImplicitly]
            class NestedSessionManager {
                internal static readonly SessionManager SessionManager =
                    new SessionManager();
            }
        }





    }
}
