using System;

namespace softWrench.sW4.SPF {

    /// <summary>
    /// should be used when there´s no result data from the action, but we still want 
    /// to perform a redirection in the single page framework.
    /// </summary>
    public class RedirectResponseResult : IGenericResponseResult {
        public string RedirectURL { get; set; }
        public string Title { get; set; }
        public string CrudSubTemplate { get; set; }
        public string SuccessMessage { get; set; }
        public long RequestTimeStamp { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
