using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using NHibernate;
using cts.commons.simpleinjector;

namespace cts.commons.persistence {
    public interface IBaseHibernateDAO: IComponent {
        IQuery BuildQuery(string queryst, object[] parameters, ISession session, bool native = false,
            string queryAlias = null);

        IQuery BuildQuery(string queryst, ISession session, bool native = false);

        IQuery BuildQuery(string queryst, ExpandoObject parameters, ISession session, bool native = false,
            IPaginationData paginationData = null, string queryAlias = null);

        List<Dictionary<string, string>> FindByNativeQuery(string queryst, params object[] parameters);

        Task<List<Dictionary<string, string>>> FindByNativeQueryAsync(string queryst, params object[] parameters);

        IList<dynamic> FindByNativeQuery(String queryst, ExpandoObject parameters,
            IPaginationData paginationData = null, string queryAlias = null);

        Task<IList<dynamic>> FindByNativeQueryAsync(String queryst, ExpandoObject parameters,
            IPaginationData paginationData = null, string queryAlias = null);

        T FindSingleByNativeQuery<T>(string queryst, params object[] parameters) where T : class;

        Task<T> FindSingleByNativeQueryAsync<T>(string queryst, params object[] parameters) where T : class;

        int CountByNativeQuery(string queryst, ExpandoObject parameters, string queryAlias = null);

        Task<int> CountByNativeQueryAsync(string queryst, ExpandoObject parameters, string queryAlias = null);

        int ExecuteSql(string sql, params object[] parameters);

        Task<int> ExecuteSqlAsync(string sql, params object[] parameters);

        void RegisterQueryObserver(IQueryObserver observer);

        ISession GetSession();
        Task<ITransaction> BeginTransactionAsync(ISession session);
    }
}