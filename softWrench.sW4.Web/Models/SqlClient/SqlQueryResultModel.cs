using System.Collections.Generic;

namespace softWrench.sW4.Web.Models.SqlClient {

    /// <summary>
    /// The SQL query model
    /// </summary>
    public class SqlQueryResultModel {

        /// <summary>
        /// Gets or sets the result set.
        /// </summary>
        public IList<dynamic> ResultSet { get; set; }

        /// <summary>
        /// Gets or sets the total execution time
        /// </summary>
        public string ExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if there was an error during execution
        /// </summary>
        public bool HasErrors { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ExecutionMessage { get; set; }
    }
}