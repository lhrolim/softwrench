using cts.commons.persistence;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;
using JetBrains.Annotations;
using log4net;
using NHibernate;
using NHibernate.Mapping.Attributes;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace softWrench.sW4.Data.Persistence.SWDB {

    public class SWDBHibernateDAO : BaseHibernateDAO, ISWDBHibernateDAO {

        private static readonly ILog Log = LogManager.GetLogger(SwConstants.SQLDB_LOG);

        public SWDBHibernateDAO(IApplicationConfiguration applicationConfiguration, HibernateUtil hibernateUtil) : base(applicationConfiguration, hibernateUtil) {
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
            using (var session = GetSession()) {
                using (var transaction = session.BeginTransaction()) {
                    ob = DoSave(ob, session);
                    transaction.Commit();
                    return ob;
                }
            }
        }

        public ICollection<T> BulkSave<T>(ICollection<T> items) where T : class {
            if (items == null || !items.Any()) {
                return items;
            }
            var result = new List<T>(items.Count);
            using (var session = GetSessionManager().OpenSession()) {
                using (var transaction = session.BeginTransaction()) {
                    // adding the saved items to a new collection 
                    // because they can't replace the original's in an iteration 
                    result.AddRange(items.Select(item => DoSave(item, session)));
                    transaction.Commit();
                }
            }
            return result;
        }


        private static T DoSave<T>(T ob, ISession session) where T : class {
            var b = ob as IBaseEntity;
            var aud = ob as IBaseAuditEntity;
            if (aud != null) {
                aud.UpdateDate = DateTime.Now;
            }

            if (b != null && (b.Id == 0 || b.Id == null)) {
                if (aud != null) {
                    aud.CreationDate = DateTime.Now;
                    aud.CreatedBy = SecurityFacade.CurrentUser().UserId;
                }
                b.Id = (int)session.Save(ob);
            } else {
                ob = session.Merge(ob);
            }
            return ob;
        }

        public void DeleteCollection(IEnumerable<object> collection) {
            using (var session = GetSession()) {
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
            using (var session = GetSession()) {
                using (var transaction = session.BeginTransaction()) {
                    session.Delete(ob);
                    transaction.Commit();
                }
            }
        }

        public T FindByPK<T>(Type type, object id, params string[] toEager) {
            using (var session = GetSession()) {
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
            using (var session = GetSession()) {
                using (session.BeginTransaction()) {
                    var query = BuildQuery(queryst, parameters, session);
                    return (T)query.UniqueResult();
                }
            }
        }


        public IList<T> FindByQuery<T>(string queryst, params object[] parameters) where T : class {
            using (var session = GetSession()) {
                using (session.BeginTransaction()) {
                    var query = BuildQuery(queryst, parameters, session);
                    return query.List<T>();
                }
            }
        }


        public IList<T> FindByNativeQuery<T>(string queryst, PaginationData paginationData = null, params object[] parameters) where T : class {
            using (var session = GetSession()) {
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



        public IList<T> FindAll<T>(Type type) where T : class {
            using (var session = GetSession()) {
                using (session.BeginTransaction()) {
                    var query = BuildQuery("from " + type.Name, (object[])null, session);
                    return query.List<T>();
                }
            }
        }

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

//        public static ISession CurrentSession() {
//            if (MaximoHibernateDAO.SessionManager.SessionFactory.IsClosed) {
//                return MaximoHibernateDAO.SessionManager.Instance.OpenSession();
//            }
//            try {
//                var currentSession = MaximoHibernateDAO.SessionManager.Instance.CurrentSession;
//                if (!currentSession.IsOpen) {
//                    return MaximoHibernateDAO.SessionManager.Instance.OpenSession();
//                }
//                return currentSession;
//            } catch (Exception) {
//                return MaximoHibernateDAO.SessionManager.Instance.OpenSession();
//            }
//        }


        #region configuration

        protected override string GetDialect() {
            return _hibernateUtil.HibernateDialect(DBType.Swdb);
        }

        protected override string GetDriverName() {
            return _hibernateUtil.HibernateDriverName(DBType.Swdb);
        }

        protected override string GetConnectionString() {
            return ApplicationConfiguration.DBConnectionString(DBType.Swdb);
        }

        protected override IEnumerable<Assembly> GetListOfAssemblies() {
            return AssemblyLocator.GetSWAssemblies();
        }

        #endregion

     
    }
}
