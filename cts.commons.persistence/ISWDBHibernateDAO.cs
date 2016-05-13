using System;
using System.Collections.Generic;
using cts.commons.simpleinjector;
using NHibernate;

namespace cts.commons.persistence
{
    public interface ISWDBHibernateDAO : IBaseHibernateDAO, ISingletonComponent
    {
        T Save<T>(T ob) where T : class;
        void DeleteCollection(IEnumerable<object> collection);
        void Delete(Object ob);
        T FindByPK<T>(Type type, object id, params string[] toEager);
        T FindSingleByQuery<T>(String queryst, params object[] parameters);
        IList<T> FindByQuery<T>(String queryst, params object[] parameters) where T : class;
        IList<T> FindAll<T>(Type type) where T : class;
        int ExecuteSql(string sql, params object[] parameters);
        ICollection<T> BulkSave<T>(ICollection<T> items) where T : class;
    }
}