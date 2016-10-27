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

        void Delete(Object ob);

        Task DeleteAsync(Object ob);


        T FindByPK<T>(Type type, object id, params string[] toEager);

        Task<T> FindByPKAsync<T>(object id, params string[] toEager);

        T FindSingleByQuery<T>(String queryst, params object[] parameters);

        Task<T> FindSingleByQueryAsync<T>(String queryst, params object[] parameters);


        IList<T> FindByQuery<T>(String queryst, params object[] parameters) where T : class;
        Task<IList<T>> FindByQueryAsync<T>(String queryst, params object[] parameters) where T : class;

        IList<T> FindAll<T>(Type type) where T : class;

        Task<IList<T>> FindAllAsync<T>() where T : class;


        ICollection<T> BulkSave<T>(IEnumerable<T> items) where T : class;

        Task<ICollection<T>> BulkSaveAsync<T>(IEnumerable<T> items) where T : class;
    }
}