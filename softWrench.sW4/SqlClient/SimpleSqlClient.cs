using System;
using cts.commons.persistence;
using softWrench.sW4.Data.Persistence;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using cts.commons.persistence.Transaction;
using cts.commons.portable.Util;
using cts.commons.Util;

namespace softWrench.sW4.SqlClient {

    /// <summary>
    /// Exclusive use for softWrench.sW4.Web.Controllers.SqlClient.SqlClientController
    /// </summary>
    public class SimpleSqlClient : ISqlClient {

        private readonly ISWDBHibernateDAO _swdbDao;
        private readonly IMaximoHibernateDAO _maximodbDao;


        public SimpleSqlClient(ISWDBHibernateDAO swdbDao, IMaximoHibernateDAO maximodbDAO) {
            _swdbDao = swdbDao;
            _maximodbDao = maximodbDAO;
        }

        public IList<dynamic> ExecuteQuery(string query, DBType dbType, int limit = 0) {
            var pagination = limit > 0 ? new PaginationData(limit, 1, string.Empty) : null;
            
            return GetDao(dbType).FindByNativeQuery(query, null, pagination);
        }

        [Transactional(DBType.Swdb, DBType.Maximo)]
        public virtual int ExecuteUpdate(string query, DBType dbType) {
            var paramsToPass = new List<object>();

//            if (parameters != null) {
//                foreach (var parameter in parameters) {
//                    if (parameter.StartsWith("@swc:")) {
//                        paramsToPass.Add(CompressionUtil.Compress(parameter.Substring(5).GetBytes()));
//                    } else {
//                        paramsToPass.Add(parameter);
//                    }
//                }
//            }

            return GetDao(dbType).ExecuteSql(query, null);
        }

        public bool IsDefinitionOrManipulation(string sql) {
            var regex = new Regex(@"(?:insert|update|delete|alter|drop|create).+", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regex.IsMatch(sql);
        }

        private IBaseHibernateDAO GetDao(DBType dbType) {
            return dbType == DBType.Swdb ? (IBaseHibernateDAO)_swdbDao : _maximodbDao;
        }
    }
}
