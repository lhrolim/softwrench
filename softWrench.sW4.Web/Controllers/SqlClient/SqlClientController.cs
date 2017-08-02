using Common.Logging;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using cts.commons.Util;
using softWrench.sW4.Security.Attributes;
using softWrench.sW4.SPF;
using softWrench.sW4.SqlClient;
using softWrench.sW4.Util;
using softWrench.sW4.Web.Models.SqlClient;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using cts.commons.persistence.Transaction;

namespace softWrench.sW4.Web.Controllers.SqlClient {
    /// <summary>
    /// The default controller class for the SQL Client
    /// </summary>
    public class SqlClientController : ApiController {

        private readonly ILog log = LogManager.GetLogger(typeof(SqlClientController));

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlClientController"/> class.
        /// </summary>
        public SqlClientController() {
        }

        [HttpGet]
        public RedirectResponseResult SqlClient() {
            var result = new RedirectResponseResult() {
                RedirectURL = string.Format(SPFRedirectAttribute.ConventionPattern , "SqlClient"),
                Title = "SQL Client"
            };
            return result;
        }

        /// <summary>
        /// Executes the raw query
        /// </summary>
        /// <param name="viewModel">the sql query viewmodel of type <see cref="SqlQueryViewModel"/></param>
        /// <returns>The <see cref="SqlQueryResultModel"/> result</returns>
        [HttpGet]
        [DynamicAdminRole]
        [Transactional(DBType.Swdb, DBType.Maximo)]
        public virtual SqlQueryResultModel ExecuteQuery (string query, string datasource, int limit) { // SqlQueryViewModel viewModel) {
            var model = new SqlQueryResultModel();
            var timer = new Stopwatch();
            timer.Start();

            try {
                if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(datasource)) {
                    model.HasErrors = true;
                    model.ExecutionMessage = "The sql query or the datasource cannot be empty.";                    
                } else {
                    var dbType = (DBType)Enum.Parse(typeof(DBType), datasource, true);

                    var sqlClient = SimpleInjectorGenericFactory.Instance.GetObject<ISqlClient>();
                    if (sqlClient.IsDefinitionOrManipulation(query)) {
                        if ((dbType == DBType.Maximo && CheckEnvironment()) || dbType == DBType.Swdb) {
                            var result = sqlClient.ExecuteUpdate(query, dbType);                           
                            model.ExecutionMessage = string.Format("{0} records(s) affected", result);
                        } else {
                            model.HasErrors = true;
                            model.ExecutionMessage = "Cannot execute this query. Access denied.";
                        }
                    } else {
                        var resultset = sqlClient.ExecuteQuery(query, dbType, limit);
                        model.ResultSet = resultset;
                        model.ExecutionMessage = resultset != null ? string.Format("{0} records(s) returned", model.ResultSet.Count()) : "No records found.";
                        model.HasErrors = false;
                    }
                }
            } catch (Exception ex) {
                model.HasErrors = true;

                if(ex.InnerException != null) {
                    model.ExecutionMessage = ex.Message + " : " + ex.InnerException.Message;
                } else {
                    model.ExecutionMessage = ex.Message;
                }
            }

            timer.Stop();

            model.ExecutionTime = string.Format("{0}:{1}:{2}:{3}", timer.Elapsed.Hours, timer.Elapsed.Minutes, timer.Elapsed.Seconds, timer.Elapsed.TotalMilliseconds);

            log.Info(LoggingUtil.BaseDurationMessage("SQL client query processed in {0}", timer));

            return model;
        }

        /// <summary>
        /// Checks the environment for sql execution
        /// </summary>
        /// <returns>True if the environment is QA or DEV; otherwise returns false;</returns>
        private bool CheckEnvironment()
        {
            return true;
        }
    }
}
