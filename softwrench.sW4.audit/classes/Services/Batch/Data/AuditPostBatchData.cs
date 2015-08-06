using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using softwrench.sW4.audit.classes.Model;
using softWrench.sW4.Data.Persistence.WS.API;

namespace softwrench.sW4.audit.classes.Services.Batch.Data {
    /// <summary>
    /// DTO class for passing around Batch and Audit related data to the PostBatchHandlers
    /// </summary>
    public class AuditPostBatchData {

        public TargetResult MaximoResult { get; private set; }
        public ICollection<AuditEntry> Entries { get; private set; }

        /// <summary>
        /// For now it only takes a BatchItem's additional data and it's submit result from Maximo.
        /// Grow it on demand.
        /// </summary>
        /// <param name="maximoResult"></param>
        /// <param name="itemAdditionalData"></param>
        public AuditPostBatchData(TargetResult maximoResult, JObject itemAdditionalData) {
            MaximoResult = maximoResult;
            Entries = EntriesFromData(itemAdditionalData);
        }

        private ICollection<AuditEntry> EntriesFromData(JObject itemAdditionalData) {
            if (!itemAdditionalData.Any()) {
                return new AuditEntry[0];
            }
            JArray entries = null;
            foreach (var item in itemAdditionalData) {
                if (item.Key == "auditentries") {
                    entries = item.Value as JArray;
                    break;
                }
            }
            if (entries == null) {
                return new AuditEntry[0];
            }
            // AuditEntry Enumerable from Array of Jsons
            return entries
                .Select(entry => AuditEntryFromJson(entry as JObject))
                .ToList();
        }

        private AuditEntry AuditEntryFromJson(JObject entryJson) {
            var entry = entryJson.ToObject<ClientAuditEntry>();

            var dataMapString = entry.Datamap != null ? entry.Datamap.ToString(Formatting.None) : null;

            return new AuditEntry(entry.Operation, entry.RefApplication, entry.RefId, entry.RefUserId, dataMapString, entry.CreatedBy, entry.CreatedDate);
        }

        class ClientAuditEntry {
            public string Operation { get; set; }
            public string RefApplication { get; set; }
            public string CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string RefId { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string RefUserId { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public JObject Datamap { get; set; }

            public ClientAuditEntry() { }
        }
    }
}
