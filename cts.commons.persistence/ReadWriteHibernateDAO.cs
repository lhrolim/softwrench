using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using JetBrains.Annotations;
using NHibernate;

namespace cts.commons.persistence {

    public abstract class ReadWriteHibernateDAO : BaseHibernateDAO, ISWDBHibernateDAO {
        protected ReadWriteHibernateDAO([NotNull]IApplicationConfiguration applicationConfiguration, HibernateUtil hibernateUtil) : base(applicationConfiguration, hibernateUtil) {
        }

        public T Save<T>(T ob) where T : class {
            return AsyncHelper.RunSync(() => SaveAsync(ob));
        }

        public async Task<T> SaveAsync<T>(T ob) where T : class {
            using (var session = GetSession()) {
                using (var transaction = await session.BeginTransactionAsync()) {
                    ob = await DoSave(ob, session);
                    await transaction.CommitAsync();
                    return ob;
                }
            }
        }


        public ICollection<T> BulkSave<T>(ICollection<T> items) where T : class {
            return AsyncHelper.RunSync(() => BulkSaveAsync<T>(items));
        }

        public async Task<ICollection<T>> BulkSaveAsync<T>(ICollection<T> items) where T : class {
            if (items == null || !items.Any()) {
                return items;
            }
            var result = new List<T>(items.Count);
            using (var session = GetSessionManager().OpenSession()) {
                using (var transaction = await session.BeginTransactionAsync()) {
                    // adding the saved items to a new collection 
                    // because they can't replace the original's in an iteration 
                    result.AddRange(await Task.WhenAll(items.Select(item => DoSave(item, session))));
                    await transaction.CommitAsync();
                }
            }
            return result;
        }


        private async Task<T> DoSave<T>(T ob, ISession session) where T : class {
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
                b.Id = (int)await session.SaveAsync(ob);
            } else {
                ob = await session.MergeAsync(ob);
            }
            return ob;
        }

        protected abstract int? GetCreatedByUser();


        public void DeleteCollection(IEnumerable<object> collection) {

        }

        public async Task DeleteCollectionAsync(IEnumerable<object> collection) {
            using (var session = GetSession()) {
                using (var transaction = await session.BeginTransactionAsync()) {
                    if (collection != null) {
                        foreach (var element in collection) {
                            await session.DeleteAsync(element);
                        }
                        await transaction.CommitAsync();
                    }
                }
            }
        }

        public void Delete(object ob) {
            AsyncHelper.RunSync(() => DeleteAsync(ob));
        }

        public async Task DeleteAsync(object ob) {
            using (var session = GetSession()) {
                using (var transaction = await session.BeginTransactionAsync()) {
                    await session.DeleteAsync(ob);
                    await transaction.CommitAsync();
                }
            }
        }

        public T FindByPK<T>(Type type, object id, params string[] toEager) {
            return AsyncHelper.RunSync(() => FindByPKAsync<T>(id, toEager));
        }

        public async Task<T> FindByPKAsync<T>(object id, params string[] toEager) {
            var type = typeof(T);
            using (var session = GetSession()) {
                using (await session.BeginTransactionAsync()) {
                    var ob = session.LoadAsync(type, id);
                    for (int i = 0; i < toEager.Length; i++) {
                        object property = BaseReflectionUtil.GetProperty(ob, toEager[i]);
                        NHibernateUtil.Initialize(property);
                    }
                    return (T)await ob;
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
            return AsyncHelper.RunSync(() =>FindSingleByQueryAsync<T>(queryst, parameters));
        }

        public async Task<T> FindSingleByQueryAsync<T>(string queryst, params object[] parameters) {
            using (var session = GetSession()) {
                using (await session.BeginTransactionAsync()) {
                    var query = BuildQuery(queryst, parameters, session);
                    return (T)await query.UniqueResultAsync();
                }
            }
        }


        public IList<T> FindByQuery<T>(string queryst, params object[] parameters) where T : class {
            return AsyncHelper.RunSync(()=>FindByQueryAsync<T>(queryst, parameters));
        }

        public async Task<IList<T>> FindByQueryAsync<T>(string queryst, params object[] parameters) where T : class {
            using (var session = GetSession()) {
                using (await session.BeginTransactionAsync()) {
                    var query = BuildQuery(queryst, parameters, session);
                    return await query.ListAsync<T>();
                }
            }
        }

        public async Task<IList<T>> FindAllAsync<T>() where T : class {
            var type = typeof(T);
            using (var session = GetSession()) {
                using (await session.BeginTransactionAsync()) {
                    var query = BuildQuery("from " + type.Name, (object[])null, session);
                    return await query.ListAsync<T>();
                }
            }
        }


        public IList<T> FindAll<T>(Type type) where T : class {
            return AsyncHelper.RunSync(FindAllAsync<T>);
        }
    }
}
