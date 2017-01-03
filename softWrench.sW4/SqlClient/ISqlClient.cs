using System.Collections.Generic;
using cts.commons.persistence;
using cts.commons.simpleinjector;

namespace softWrench.sW4.SqlClient {

    /// <summary>
    /// Interface for the sql client
    /// </summary>
    public interface ISqlClient : ISingletonComponent {
        /// <summary>
        /// Executes the raw sql query
        /// </summary>
        /// <param name="query">the sql query</param>
        /// <param name="dbType"></param>
        /// <param name="limit"></param>
        /// <returns>The resultset</returns>
        IList<dynamic> ExecuteQuery(string query, DBType dbType, int limit = 0);

        /// <summary>
        /// Executes the raw sql query
        /// </summary>
        /// <param name="query">the sql query</param>
        /// <param name="dbType"></param>
        /// <returns>The rows affected count</returns>
        int ExecuteUpdate(string query, DBType dbType);

        /// <summary>
        /// Checks if the sql string has DDL or DML operations
        /// </summary>
        /// <param name="sql">The sql string</param>
        /// <returns>True if the sql has CRUD operations; otherwise returns false.</returns>
        bool IsDefinitionOrManipulation(string sql);
    }
}
