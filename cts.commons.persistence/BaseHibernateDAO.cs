using cts.commons.portable.Util;
using log4net;
using NHibernate;
using NHibernate.Transform;
using NHibernate.Type;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence.Event;
using cts.commons.persistence.Util;
using cts.commons.simpleinjector.app;
using cts.commons.Util;
using JetBrains.Annotations;

namespace cts.commons.persistence {


    public abstract class BaseHibernateDAO : ISingletonComponent, ISWEventListener<RestartDBEvent> {

        private static readonly ILog HibernateLog = LogManager.GetLogger(typeof(BaseHibernateDAO));

        private readonly IApplicationConfiguration _applicationConfiguration;


        public BaseHibernateDAO(IApplicationConfiguration applicationConfiguration) {
            _applicationConfiguration = applicationConfiguration;
        }


        public IQuery BuildQuery(string queryst, object[] parameters, ISession session, bool native = false,string queryAlias=null) {
            var result = HibernateUtil.TranslateQueryString(queryst, parameters);
            queryst = result.query;
            parameters = result.Parameters;

            var query = native ? session.CreateSQLQuery(queryst) : session.CreateQuery(queryst);
            query.SetFlushMode(FlushMode.Never);
            LogQuery(queryst, queryAlias,parameters);
            if (result.Parameters == null) {
                return query;
            }
            for (var i = 0; i < result.Parameters.Length; i++) {
                if (queryst.Contains(":p" + i)) {
                    if (parameters[i] == null) {
                        query.SetParameter("p" + i, parameters[i], NHibernateUtil.String);
                    } else {
                        var asEnumerable = parameters[i] as IEnumerable;
                        if (asEnumerable != null) {
                            query.SetParameterList("p" + i, (IEnumerable)parameters[i]);
                        } else {
                            query.SetParameter("p" + i, parameters[i]);
                        }
                    }
                } else {
                    if (parameters[i] != null) {
                        if (parameters[i] is Int32) {
                            query.SetParameter(i, parameters[i], new Int32Type());
                        } else {
                            query.SetParameter(i, parameters[i]);
                        }
                    }

                }

            }

            return query;
        }


        public IQuery BuildQuery(string queryst, ISession session, bool native = false) {
            LogQuery(queryst, null);
            var query = native ? session.CreateSQLQuery(queryst) : session.CreateQuery(queryst);

            return query;
        }

        public IQuery BuildQuery(string queryst, ExpandoObject parameters, ISession session, bool native = false, IPaginationData paginationData = null,string queryAlias=null) {
            LogQuery(queryst,queryAlias, parameters);
            if (paginationData != null && _applicationConfiguration.IsDB2(DBType.Maximo)) {
                //nhibernate pagination breaks in some scenarios, at least in DB2, keeping others intact for now
                queryst = NHibernatePaginationUtil.ApplyManualPaging(queryst, paginationData);
            }
            LogPaginationQuery(queryst,queryAlias, parameters);

            var query = native ? session.CreateSQLQuery(queryst) : session.CreateQuery(queryst);

            if (!_applicationConfiguration.IsDB2(DBType.Maximo) && paginationData != null) {
                var pageSize = paginationData.PageSize;
                query.SetMaxResults(pageSize);
                query.SetFirstResult((paginationData.PageNumber - 1) * pageSize);
            }
            if (parameters == null) {
                return query;
            }
            foreach (var parameter in parameters) {
                if (parameter.Value.GetType().IsGenericType && parameter.Value is IEnumerable) {
                    var list = new List<string>();
                    list.AddRange((IEnumerable<string>)parameter.Value);
                    if (query.NamedParameters.Contains(parameter.Key)) {
                        query.SetParameterList(parameter.Key, list);
                    }
                } else {
                    // TODO: This is wrong!!! The start and end date should be 2 diferent parameters. REFACTOR LATER!!!
                    if (parameter.Key.IndexOf("___", StringComparison.Ordinal) == -1) {

                        if (query.NamedParameters.Contains(parameter.Key)) {
                            query.SetParameter(parameter.Key, parameter.Value);
                        }

                    } else {

                        var startDate = DateUtil.Parse(parameter.Value.ToString().Split(new[] { "___" },
                            StringSplitOptions.RemoveEmptyEntries)[0]);
                        var endDate = DateUtil.Parse(parameter.Value.ToString().Split(new[] { "___" },
                            StringSplitOptions.RemoveEmptyEntries)[1]);

                        query.SetParameter(parameter.Key + "_start", DateUtil.BeginOfDay(startDate.Value));
                        query.SetParameter(parameter.Key + "_end", DateUtil.EndOfDay(endDate.Value));
                    }
                }
            }
            return query;
        }


        [NotNull]
        public List<Dictionary<string, string>> FindByNativeQuery(String queryst, params object[] parameters) {
            IList<dynamic> queryResult;
            using (var session = GetSessionManager().OpenSession()) {
                using (session.BeginTransaction()) {
                    var query = BuildQuery(queryst, parameters, session, true);
                    query.SetResultTransformer(NhTransformers.ExpandoObject);
                    queryResult = query.List<dynamic>();
                }
            }
            if (queryResult == null || !queryResult.Any()) {
                return new List<Dictionary<string, string>>();
            }
            var list = queryResult.Cast<IEnumerable<KeyValuePair<string, object>>>()
                .Select(r => r.ToDictionary(pair => pair.Key, pair => (pair.Value == null ? null : pair.Value.ToString()), StringComparer.OrdinalIgnoreCase))
               .ToList();
            return list;
        }


        public IList<dynamic> FindByNativeQuery(String queryst, ExpandoObject parameters, IPaginationData paginationData = null, string queryAlias=null) {
            var before = Stopwatch.StartNew();
            using (var session = GetSessionManager().OpenSession()) {
                var query = BuildQuery(queryst, parameters, session, true, paginationData);
                query.SetResultTransformer(NhTransformers.ExpandoObject);
                var result = query.List<dynamic>();
                GetLog().Debug(LoggingUtil.BaseDurationMessageFormat(before, "{0}: done query".Fmt(queryAlias ?? "")));
                return result;
            }
        }

        public T FindSingleByNativeQuery<T>(String queryst, params object[] parameters) where T : class {
            using (var session = GetSessionManager().OpenSession()) {
                using (session.BeginTransaction()) {
                    var query = BuildQuery(queryst, parameters, session, true);
                    return (T)query.UniqueResult();
                }
            }
        }

        //public int FindSingleByQuery(String queryst, params object[] parameters)  {
        //    using (var session = GetSessionManager().OpenSession()) {
        //        using (session.BeginTransaction()) {
        //            var query = BuildQuery(queryst, parameters, session);
        //            return (int)query.UniqueResult();
        //        }
        //    }
        //}
        public int CountByNativeQuery(String queryst, ExpandoObject parameters, string queryAlias = null) {
            var before = Stopwatch.StartNew();
            using (var session = GetSessionManager().OpenSession()) {
                var query = BuildQuery(queryst, parameters, session, true, null, queryAlias);
                var result = Convert.ToInt32(query.UniqueResult());
                GetLog().Debug(LoggingUtil.BaseDurationMessageFormat(before, "{0}: done count query".Fmt(queryAlias ?? "")));
                return result;
            }
        }



        public static class NhTransformers {
            public static readonly IResultTransformer ExpandoObject;

            static NhTransformers() {
                ExpandoObject = new ExpandoObjectResultSetTransformer();
            }

            private class ExpandoObjectResultSetTransformer : IResultTransformer {
                public IList TransformList(IList collection) {
                    return collection;
                }

                public object TransformTuple(object[] tuple, string[] aliases) {
                    var expando = new ExpandoObject();
                    var dictionary = (IDictionary<string, object>)expando;
                    for (int i = 0; i < tuple.Length; i++) {
                        string alias = aliases[i];
                        if (alias != null) {
                            dictionary[alias] = tuple[i];
                        }
                    }
                    return expando;
                }
            }
        }


        protected abstract ILog GetLog();


        private void LogQuery(string queryst,string queryAlias, params object[] parameters) {
            if (!GetLog().IsDebugEnabled) {
                return;
            }
            GetLog().Debug(LoggingUtil.QueryStringForLogging(queryst,queryAlias, parameters));
        }

        private void LogPaginationQuery(string queryst,string queryAlias, params object[] parameters) {
            if (!HibernateLog.IsDebugEnabled) {
                return;
            }
            HibernateLog.Debug(LoggingUtil.QueryStringForLogging(queryst,queryAlias, parameters));
        }

        public interface ISessionManager {
            ISession OpenSession();

            void Restart();

        }

        protected abstract ISessionManager GetSessionManager();


        public void HandleEvent(RestartDBEvent eventToDispatch) {
            GetSessionManager().Restart();
        }
    }
}
