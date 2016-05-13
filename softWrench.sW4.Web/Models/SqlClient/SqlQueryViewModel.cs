namespace softWrench.sW4.Web.Models.SqlClient {

    /// <summary>
    /// Class to define the SQL query information from the SQL Client.
    /// </summary>
    public class SqlQueryViewModel {

        /// <summary>
        /// Gets ot sets the raw sql query
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the execution profile 
        /// </summary>
        public string Profile { get; set; }
    }
}