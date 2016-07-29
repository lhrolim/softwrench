﻿using cts.commons.persistence;
using cts.commons.simpleinjector;
using NHibernate;
using softWrench.sW4.Data.Persistence;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace softWrench.sW4.SqlClient {
    public class SimpleSqlClient : ISqlClient {

        private readonly IBaseHibernateDAO dao;

        public SimpleSqlClient(IBaseHibernateDAO dao) {
            this.dao = dao;
        }

        public IList<dynamic> ExecuteQuery(string query, int limit = 0) {
            var pagination = limit > 0 ? new PaginationData(limit, 1, string.Empty) : null;
            return dao.FindByNativeQuery(query, null, pagination); 
        }

        public int ExecuteUpdate(string query) {
            return dao.ExecuteSql(query, null);
        }

        public bool IsDefinitionOrManipulation(string sql) {
            var regex = new Regex(@"(?:insert|update|delete|alter|drop|create).+", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regex.IsMatch(sql);
        }
    }
}
