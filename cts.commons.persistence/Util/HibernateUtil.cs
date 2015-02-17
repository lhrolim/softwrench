using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.app;

namespace cts.commons.persistence.Util {
    public class HibernateUtil : ISingletonComponent {

        public const string ParameterPrefix = ":";
        public const string ListParameterPrefixPattern = "(:{0})";
        public const string ParameterPrefixPattern = ":{0}";

        private readonly IApplicationConfiguration _applicationConfiguration;

        private static HibernateUtil _instance;

        public HibernateUtil(IApplicationConfiguration applicationConfiguration) {
            _applicationConfiguration = applicationConfiguration;
        }

        public static HibernateUtil GetInstance() {
            if (_instance == null) {
                _instance = SimpleInjectorGenericFactory.Instance.GetObject<HibernateUtil>(typeof(HibernateUtil));
            }
            return _instance;
        }


        public string HibernateDriverName(DBType dbtype) {

            DBMS? dbms = _applicationConfiguration.LookupDBMS(dbtype);
            if (dbms == DBMS.MSSQL) {
                return typeof(NHibernate.Driver.SqlClientDriver).FullName;
            }
            if (dbms == DBMS.DB2) {
                return typeof(NHibernate.Driver.DB2Driver).FullName;
            }
            if (dbms == DBMS.ORACLE) {
                return typeof(NHibernate.Driver.OracleClientDriver).FullName;
            }
            if (dbms == DBMS.MYSQL) {
                return typeof(NHibernate.Driver.MySqlDataDriver).FullName;
            }
            return typeof(NHibernate.Driver.SqlClientDriver).FullName;
        }


        public string HibernateDialect(DBType dbtype) {
            DBMS? dbms = _applicationConfiguration.LookupDBMS(dbtype);
            if (dbms == DBMS.MSSQL) {
                return typeof(NHibernate.Dialect.MsSql2008Dialect).FullName;
            }
            if (dbms == DBMS.DB2) {
                return typeof(NHibernate.Dialect.DB2Dialect).FullName;
            }
            if (dbms == DBMS.ORACLE) {
                return typeof(NHibernate.Dialect.Oracle10gDialect).FullName;
            }
            if (dbms == DBMS.MYSQL) {
                return typeof(NHibernate.Dialect.MySQLDialect).FullName;
            }
            return typeof(NHibernate.Dialect.MsSql2008Dialect).FullName;
        }
        /// <summary>
        /// We need this method to translate the queries for each null parameter, as nhibernate gets lost with that. In these cases, it should be is null instead of =?
        /// </summary>
        /// <param name="queryst"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static QueryTranslateResult TranslateQueryString(string queryst, params object[] parameters) {
            if (parameters == null || parameters.All(p => p != null)) {
                return new QueryTranslateResult(queryst, parameters);
            }
            var resultParameters = new List<object>();
            var sb = new StringBuilder(queryst);
            var resultString = queryst;
            var currentMark = queryst.IndexOf('?');
            var i = 0;

            var equalIdxs = new List<int>();
            var markIdxs = new List<int>();

            var currentEqual = 0;
            while (currentMark != -1) {
                currentEqual = queryst.IndexOf('=', currentEqual + 1);
                if (parameters[i] == null) {
                    sb[currentEqual] = '#';
                    sb[currentMark] = '$';
                } else {
                    resultParameters.Add(parameters[i]);
                }
                if (queryst.Length >= currentMark + 1) {
                    currentMark = queryst.IndexOf('?', currentMark + 1);
                    if (currentMark == -1) {
                        break;
                    }
                } else {
                    break;
                }
                i++;
            }
            sb.Replace('#'.ToString(), " is ");
            sb.Replace('$'.ToString(), " null ");

            return new QueryTranslateResult(sb.ToString(), resultParameters);


        }

        public class QueryTranslateResult {
            public String query;
            public object[] Parameters;

            public QueryTranslateResult(string query, IEnumerable<object> parameters) {
                this.query = query;
                if (parameters != null) {
                    Parameters = parameters.ToArray();
                }
            }
        }

    }
}
