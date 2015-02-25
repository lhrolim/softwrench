﻿using System;

namespace softWrench.sW4.SPF {
    public interface IGenericResponseResult {
        string RedirectURL { get; set; }

        string Title { get; set; }
        
        /// <summary>
        /// temporary solution to specify pages that should be included inside the crud_body. every page should be this way in the future, 
        /// so that just the RedirectURL should be necessary
        /// </summary>
        string CrudSubTemplate { get; set; }

        string SuccessMessage { get; set; }

        /// <summary>
        /// the time the request was made
        /// </summary>
        long RequestTimeStamp { get; set; }

        /// <summary>
        /// used for angularjs watch expression on a single value
        /// </summary>
        DateTime TimeStamp { get; set; }
    }
}