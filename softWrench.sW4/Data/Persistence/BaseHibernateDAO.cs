using DocumentFormat.OpenXml.Spreadsheet;
using log4net;
using NHibernate;
using NHibernate.Transform;
using NHibernate.Type;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Properties;
using softWrench.sW4.Security.Services;
using softwrench.sw4.Shared2.Util;
using softWrench.sW4.SimpleInjector;
using softWrench.sW4.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;

namespace softWrench.sW4.Data.Persistence {


    public abstract class BaseHibernateDAO : ISingletonComponent {

        private static readonly ILog HibernateLog = LogManager.GetLogger("PAGINATION.SQL");

        public IQuery BuildQuery(string queryst, object[] parameters, ISession session, bool native = false, string queryAlias=null) {
            var result = HibernateUtil.TranslateQueryString(queryst, parameters);
            queryst = result.query;
            parameters = result.Parameters;

            var query = native ? session.CreateSQLQuery(queryst) : session.CreateQuery(queryst);
            if (!native) {
                query.SetFlushMode(FlushMode.Never);
            }
            query.SetTimeout(MetadataProvider.GlobalProperties.QueryTimeout());
            LogQuery(queryst,queryAlias, parameters);
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
            query.SetTimeout(MetadataProvider.GlobalProperties.QueryTimeout());
            return query;
        }

        public IQuery BuildQuery(string queryst, ExpandoObject parameters, ISession session, bool native = false, PaginationData paginationData = null, string queryAlias=null) {
            LogQuery(queryst, queryAlias,parameters);
            if (paginationData != null && ApplicationConfiguration.IsDB2(ApplicationConfiguration.DBType.Maximo)) {
                //nhibernate pagination breaks in some scenarios, at least in DB2, keeping others intact for now
                queryst = NHibernatePaginationUtil.ApplyManualPaging(queryst, paginationData);
            }
            LogPaginationQuery(queryst,queryAlias, parameters);

            var query = native ? session.CreateSQLQuery(queryst) : session.CreateQuery(queryst);
            query.SetTimeout(MetadataProvider.GlobalProperties.QueryTimeout());

            if (!ApplicationConfiguration.IsDB2(ApplicationConfiguration.DBType.Maximo) && paginationData != null) {
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
                    if (parameter.Key.IndexOf("___") == -1) {

                        if (query.NamedParameters.Contains(parameter.Key)) {
                            query.SetParameter(parameter.Key, parameter.Value);
                        }

                    } else {
                        var startDateString = parameter.Value.ToString().Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries)[0];
                        var startDate = DateUtil.Parse(startDateString);
                        var inMemoryUser = SecurityFacade.CurrentUser();
                        if (startDate.HasValue) {
                            startDate = startDate.Value.FromUserToRightKind(inMemoryUser);
                            if (!startDateString.Contains(":")) {
                                startDate = DateUtil.BeginOfDay(startDate.Value);
                            }
                        }
                        var endDateString = parameter.Value.ToString().Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries)[1];
                        var endDate = DateUtil.Parse(endDateString);
                        if (endDate.HasValue) {
                            endDate = endDate.Value.FromUserToRightKind(inMemoryUser);
                            if (!endDateString.Contains(":")) {
                                endDate = DateUtil.BeginOfDay(endDate.Value);
                            }
                        }
                        

                        query.SetParameter(parameter.Key + "_start", startDate);
                        query.SetParameter(parameter.Key + "_end", endDate);
                    }
                }
            }
            return query;
        }

        public IList<dynamic> FindByNativeQuery(String queryst, params object[] parameters) {
            using (var session = GetSessionManager().OpenSession()) {
                var query = BuildQuery(queryst, parameters, session, true);
                query.SetResultTransformer(NhTransformers.ExpandoObject);
                return query.List<dynamic>();
            }
        }

        public IList<dynamic> FindByNativeQuery(String queryst) {
            using (var session = GetSessionManager().OpenSession()) {
                var query = BuildQuery(queryst, session, true);
                query.SetResultTransformer(NhTransformers.ExpandoObject);
                return query.List<dynamic>();
            }
        }

        public IList<dynamic> FindByNativeQuery(String queryst, ExpandoObject parameters, PaginationData paginationData = null, string queryAlias=null) {
            var before = Stopwatch.StartNew();
            using (var session = GetSessionManager().OpenSession()) {
                var query = BuildQuery(queryst, parameters, session, true, paginationData,queryAlias);
                query.SetResultTransformer(NhTransformers.ExpandoObject);
                var result = query.List<dynamic>();
                GetLog().Debug(LoggingUtil.BaseDurationMessageFormat(before, "{0}: done query".Fmt(queryAlias ?? "")));
                return result;
            }
        }




        public int CountByNativeQuery(String queryst, ExpandoObject parameters, string queryAlias = null) {
            var before = Stopwatch.StartNew();

            using (var session = GetSessionManager().OpenSession()) {
                var query = BuildQuery(queryst, parameters, session, true,null,queryAlias);
                var result = (int)query.UniqueResult();
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


        private void LogQuery(string queryst, string queryAlias,params object[] parameters) {
            if (!GetLog().IsDebugEnabled) {
                return;
            }
            GetLog().Debug(LoggingUtil.QueryStringForLogging(queryst, queryAlias,parameters));
        }

        private void LogPaginationQuery(string queryst, string queryAlias, params object[] parameters) {
            if (!HibernateLog.IsDebugEnabled) {
                return;
            }
            HibernateLog.Debug(LoggingUtil.QueryStringForLogging(queryst, queryAlias,parameters));
        }

        public interface ISessionManager {
            ISession OpenSession();
        }

        protected abstract ISessionManager GetSessionManager();





    }
}
