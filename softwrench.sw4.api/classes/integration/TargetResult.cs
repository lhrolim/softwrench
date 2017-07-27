using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using softwrench.sw4.api.classes.integration;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Metadata.Applications;

namespace softWrench.sW4.Data.Persistence.WS.API {
 
    public class TargetResult {
        // Unique Id use to identify the record; it might be different than the userid
        public string Id { get; set; }
        public string UserId { get; set; }
        public object ResultObject { get; set; }
        public string SuccessMessage { get; set; }
        public string WarnMessage { get; set; }

        public IErrorDto WarningDto { get; set; }

        public ReloadMode? ReloadMode { get; set; }

        public bool FullRefresh {
            get; set;
        }

        /// <summary>
        /// List of compositions to be fetched in case a ReloadMode.MainDetail takes place
        /// </summary>
        [CanBeNull]
        public List<String> CompositionList { get; set; }

        /// <summary>
        /// Parameters to be propagated internally, that can be used by custom implementations.
        /// </summary>
        public IDictionary<string, object> ExtraParameters { get; set; } = new Dictionary<string, object>();

        public ApplicationMetadata NextApplication { get; set; }

        public string NextController { get; private set; }

        public string NextAction { get; private set; }
        public string SiteId { get; set; }

        public TargetResult(string id, string userId, object resultObject, string successMessage = null,string siteId=null) {
            Id = id;
            UserId = userId;
            ResultObject = resultObject;
            SuccessMessage = successMessage;
            SiteId = siteId;
        }
    }
}
