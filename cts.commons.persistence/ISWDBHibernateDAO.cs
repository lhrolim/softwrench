using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using NHibernate;

namespace cts.commons.persistence
{
    public interface ISWDBHibernateDAO : IBaseHibernateDAO, ISingletonComponent
    {
        T Save<T>(T ob) where T : class;
        Task<T> SaveAsync<T>(T ob) where T : class;


        void DeleteCollection(IEnumerable<object> collection);
        Task DeleteCollectionAsync(IEnumerable<object> collection);

        void Delete(object ob);

        Task DeleteAsync(object ob);


        T FindByPK<T>(Type type, object id, params string[] toEager);

        Task<T> FindByPKAsync<T>(object id, params string[] toEager);

        T FindSingleByQuery<T>(string queryst, params object[] parameters);

        Task<T> FindSingleByQueryAsync<T>(string queryst, params object[] parameters);


        IList<T> FindByQuery<T>(string queryst, params object[] parameters) where T : class;
        Task<IList<T>> FindByQueryAsync<T>(string queryst, params object[] parameters) where T : class;

        Task<IList<T>> FindByQueryWithLimitAsync<T>(string queryst,int limit, params object[] parameters) where T : class;

        IList<T> FindAll<T>(Type type) where T : class;

        Task<IList<T>> FindAllAsync<T>() where T : class;


        ICollection<T> BulkSave<T>(IEnumerable<T> items) where T : class;

        Task<ICollection<T>> BulkSaveAsync<T>(IEnumerable<T> items) where T : class;

        
    }
}