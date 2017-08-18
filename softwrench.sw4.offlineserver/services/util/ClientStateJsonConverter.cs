using System;
using System.Collections.Generic;
using Iesi.Collections.Generic;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batch.api.entities;
using softwrench.sw4.offlineserver.dto.association;

namespace softwrench.sw4.offlineserver.services.util {
    public class ClientStateJsonConverter {
        public class AppRowstampDTO {
            public IDictionary<string, string> ClientState {
                get; set;
            }
            public string MaxRowstamp {
                get; set;
            }
        }

        public static AppRowstampDTO ConvertJSONToDict(JObject rowstampMap) {
            var result = new AppRowstampDTO();
            if (rowstampMap == null || !rowstampMap.HasValues) {
                return new AppRowstampDTO();
            }
            dynamic obj = rowstampMap;
            var resultDict = new Dictionary<string, string>();
            result.ClientState = resultDict;
            result.MaxRowstamp = obj.maxrowstamp;
            if (obj.items == null) {
                return result;
            }
            //Loop over the array
            foreach (dynamic row in obj.items) {
                var id = row.id;
                var rowstamp = row.rowstamp;
                if (id.Value != null) {
                    //new items, generated on the client side, do not need to be included
                    resultDict.Add(id.Value, "" + rowstamp.Value);
                }
            }

            return result;
        }

        public static IDictionary<string, long?> GetCompositionRowstampsDict(JObject rowstampMap) {
            if (rowstampMap == null || !rowstampMap.HasValues) {
                return new Dictionary<string, long?>();
            }
            var result = new Dictionary<string, long?>();
            dynamic obj = rowstampMap;
            //Loop over the array
            foreach (dynamic row in obj.compositionmap) {
                var application = row.Name;
                result.Add(application, row.Value.Value);
            }
            return result;
        }

        public static IDictionary<string, ClientAssociationCacheEntry> GetAssociationRowstampDict(JObject rowstampMap) {
            if (rowstampMap == null || !rowstampMap.HasValues) {
                return new Dictionary<string, ClientAssociationCacheEntry>();
            }
            var result = new Dictionary<string, ClientAssociationCacheEntry>();
            dynamic obj = rowstampMap;
            //Loop over the array
            foreach (dynamic row in obj.associationmap) {
                var application = row.Name;
                var value = row.Value;
                object maxRowstamp = null;
                if (value.maximorowstamp != null) {
                    maxRowstamp = value.maximorowstamp.Value;
                }

                object maxuid = null;
                if (value.maximouid != null) {
                    maxuid = value.maximouid.Value;
                }

                //TODO: implement other fields
                result.Add(application, new ClientAssociationCacheEntry() {
                    MaxRowstamp = "" + maxRowstamp,
                    MaxUid = "" + maxuid
                });
            }
            return result;
        }

        public static ISet<BatchItem> GetBatchItems(JObject batchContent) {
            ISet<BatchItem> result = new LinkedHashSet<BatchItem>();
            dynamic obj = batchContent;
            foreach (dynamic item in obj.items) {
                JObject dm = item.datamap;
                var batchItem = new BatchItem {
                    DataMapJsonAsString = dm.ToString(),
                    ItemId = item.itemId.Value,
                    Application = item.application.Value,
                    RemoteId = item.remoteId.Value,
                    Status = BatchStatus.SUBMITTING,
                    UpdateDate = DateTime.Now,
                    Schema = "detail",
                    Operation = item.operation,
                    AdditionalData = item.additionaldata
                };
                result.Add(batchItem);
            }
            return result;
        }
    }
}
