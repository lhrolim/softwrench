
using System.Collections.Generic;
using softwrench.sW4.audit.classes.Model;
using softwrench.sW4.audit.classes.Services.Batch.Data;
using softwrench.sW4.audit.Interfaces;

namespace softwrench.sW4.audit.classes.Services.Batch {

    /// <summary>
    /// Default (client, schema and application agnostic) implementation of IAuditPostBatchHandler interface.
    /// </summary>
    public class AuditPostBatchHandler : IAuditPostBatchHandler {

        #region Properties
        public string ApplicationName() {
            return null;
        }

        public string ClientFilter() {
            return null;
        }

        public string SchemaId() {
            return null;
        }

        private readonly IAuditManager _manager;
        #endregion

        #region Constructors
        public AuditPostBatchHandler(IAuditManager manager) {
            _manager = manager;
        }
        #endregion

        /// <summary>
        /// Updates the auditentries's RefId and RefUserId as the maximo's result's Id and UserId respectively and saves them.
        /// </summary>
        /// <param name="data"></param>
        public void HandlePostBatchAuditData(AuditPostBatchData data) {
            var entries = data.Entries;
            var result = data.MaximoResult;
            foreach (var entry in entries) {
                entry.RefId = result.Id;
                entry.RefUserId = result.UserId;
            }
            _manager.SaveAuditEntries(entries);
        }
    }
}
