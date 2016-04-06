namespace softwrench.sw4.dashboard.classes.service.statistics {
    public class StatisticsRequest {
        /// <summary>
        /// Name of the entity to query statistics data.
        /// </summary>
        public string Entity { get; set; }
        /// <summary>
        /// Name of the <see cref="Entity"/>'s application.
        /// </summary>
        public string Application { get; set; }
        /// <summary>
        /// Name of the <see cref="Entity"/>'s property on which to query statistics data.
        /// </summary>
        public string Property { get; set; }
        /// <summary>
        /// Number of top value entries to fetch (like a query's page size/limit).
        /// </summary>
        public int Limit { get; set; }
        /// <summary>
        /// MetadataId of a WhereClause to be applied alongside the statistics query (useful for filtering unwanted data).
        /// </summary>
        public string WhereClauseMetadataId { get; set; }
        /// <summary>
        /// Label to be used in case a result entry has a <code>null</code> value.
        /// </summary>
        public string NullValueLabel { get; set; }
    }
}
