using System;
using System.Collections.Generic;
using System.Dynamic;
using NHibernate;

namespace cts.commons.persistence {
    public interface IBaseHibernateDAO {
        IQuery BuildQuery(string queryst, object[] parameters, ISession session, bool native = false,
            string queryAlias = null);

        IQuery BuildQuery(string queryst, ISession session, bool native = false);

        IQuery BuildQuery(string queryst, ExpandoObject parameters, ISession session, bool native = false,
            IPaginationData paginationData = null, string queryAlias = null);

        List<Dictionary<string, string>> FindByNativeQuery(string queryst, params object[] parameters);

        IList<dynamic> FindByNativeQuery(String queryst, ExpandoObject parameters,
            IPaginationData paginationData = null, string queryAlias = null);

        T FindSingleByNativeQuery<T>(string queryst, params object[] parameters) where T : class;

        int CountByNativeQuery(string queryst, ExpandoObject parameters, string queryAlias = null);

    }
}