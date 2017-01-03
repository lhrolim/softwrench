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
            return await RunTransactionalAsync(async (p) => {
                ob = await DoSave(ob, p.Session);
                await CommitTransactionAsync(p);
                return ob;
            });
        }


        public ICollection<T> BulkSave<T>(IEnumerable<T> items) where T : class {
            return AsyncHelper.RunSync(() => BulkSaveAsync<T>(items));
        }

        public async Task<ICollection<T>> BulkSaveAsync<T>(IEnumerable<T> items) where T : class {
            if (items == null || !items.Any()) {
                return items.ToList<T>();
            }
            var result = new List<T>(items.Count<T>());

            result = await RunTransactionalAsync(async (p) => {
                // adding the saved items to a new collection 
                // because they can't replace the original's in an iteration 
                foreach (var item in items) {
                    result.Add(await DoSave(item, p.Session));
                }
                await CommitTransactionAsync(p);
                return result;
            });
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
            AsyncHelper.RunSync(() => DeleteCollectionAsync(collection));
        }

        public async Task DeleteCollectionAsync(IEnumerable<object> collection) {
            await RunTransactionalAsync(async (p) => {
                if (collection != null) {
                    foreach (var element in collection) {
                        await p.Session.DeleteAsync(element);
                    }
                    await CommitTransactionAsync(p);
                }
            });
        }

        public void Delete(object ob) {
            AsyncHelper.RunSync(() => DeleteAsync(ob));
        }

        public async Task DeleteAsync(object ob) {
            await RunTransactionalAsync(async (p) => {
                await p.Session.DeleteAsync(ob);
                await CommitTransactionAsync(p);
            });
        }

        public T FindByPK<T>(Type type, object id, params string[] toEager) {
            return AsyncHelper.RunSync(() => FindByPKAsync<T>(id, toEager));
        }

        public async Task<T> FindByPKAsync<T>(object id, params string[] toEager) {
            var type = typeof(T);
            return await RunTransactionalAsync(async (p) => {
                var ob = p.Session.LoadAsync(type, id);
                for (int i = 0; i < toEager.Length; i++) {
                    object property = BaseReflectionUtil.GetProperty(ob, toEager[i]);
                    NHibernateUtil.Initialize(property);
                }
                return (T)await ob;
            });
        }

        public T EagerFindByPK<T>(Type type, object id) {
            return RunTransactional((p) => {
                var ob = p.Session.Get(type, id);
                return (T)ob;
            });
        }


        public T FindSingleByQuery<T>(string queryst, params object[] parameters) {
            return AsyncHelper.RunSync(() => FindSingleByQueryAsync<T>(queryst, parameters));
        }

        public async Task<T> FindSingleByQueryAsync<T>(string queryst, params object[] parameters) {
            return await RunTransactionalAsync(async (p) => {
                var query = BuildQuery(queryst, parameters, p.Session);
                return (T)await query.UniqueResultAsync();
            });
        }


        public IList<T> FindByQuery<T>(string queryst, params object[] parameters) where T : class {
            return AsyncHelper.RunSync(() => FindByQueryAsync<T>(queryst, parameters));
        }

        public async Task<IList<T>> FindByQueryAsync<T>(string queryst, params object[] parameters) where T : class {
            return await RunTransactionalAsync(async (p) => {
                var query = BuildQuery(queryst, parameters, p.Session);
                return await query.ListAsync<T>();
            });
        }

        public async Task<IList<T>> FindByQueryWithLimitAsync<T>(string queryst, int limit, params object[] parameters) where T : class {
            return await RunTransactionalAsync(async (p) => {
                var query = BuildQuery(queryst, parameters, p.Session);
                query.SetMaxResults(limit);
                return await query.ListAsync<T>();
            });
        }

        public async Task<IList<T>> FindAllAsync<T>() where T : class {
            var type = typeof(T);
            return await RunTransactionalAsync(async (p) => {
                var query = BuildQuery("from " + type.Name, (object[])null, p.Session);
                return await query.ListAsync<T>();
            });
        }


        public IList<T> FindAll<T>(Type type) where T : class {
            return AsyncHelper.RunSync(FindAllAsync<T>);
        }
    }
}
