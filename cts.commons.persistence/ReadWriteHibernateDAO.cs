using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using JetBrains.Annotations;
using NHibernate;

namespace cts.commons.persistence {

    public abstract class ReadWriteHibernateDAO :BaseHibernateDAO{
        protected ReadWriteHibernateDAO([NotNull]IApplicationConfiguration applicationConfiguration,HibernateUtil hibernateUtil) : base(applicationConfiguration, hibernateUtil) {
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


        private  T DoSave<T>(T ob, ISession session) where T : class {
            var b = ob as IBaseEntity;
            var aud = ob as IBaseAuditEntity;
            if (aud != null) {
                aud.UpdateDate = DateTime.Now;
            }

            if (b != null && (b.Id == 0 || b.Id == null)) {
                if (aud != null) {
                    aud.CreationDate = DateTime.Now;
                    aud.CreatedBy = GetCreatedByUser();
                }
                b.Id = (int)session.Save(ob);
            } else {
                ob = session.Merge(ob);
            }
            return ob;
        }

        protected abstract int? GetCreatedByUser();
        

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

        public void Delete(object ob) {
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
                        object property = BaseReflectionUtil.GetProperty(ob, toEager[i]);
                        NHibernateUtil.Initialize(property);
                    }
                    return (T)ob;
                }
            }
        }

        public T EagerFindByPK<T>(Type type, object id) {
            using (var session = GetSession()) {
                using (session.BeginTransaction()) {
                    var ob = session.Get(type, id);
                    return (T)ob;
                }
            }
        }


        public T FindSingleByQuery<T>(string queryst, params object[] parameters) {
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




    }
}
