namespace softwrench.sw4.dashboard.classes.service.graphic.tableau {
    public class TableauAuthDto : IGraphicStorageSystemAuthDto {
        /// <summary>
        /// Trusted ticket for the JS api
        /// </summary>
        public string Ticket { get; set; }
        /// <summary>
        /// Authentication token for the REST api 
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Tableau server url
        /// </summary>
        public string ServerUrl { get; set; }
        /// <summary>
        /// Authenticated user's id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Tableau site the user is authenticated to
        /// </summary>
        public string SiteName { get; set; }
        /// <summary>
        /// Tableau site's id the user is authenticated to
        /// </summary>
        public string SiteId { get; set; }
    }
}
