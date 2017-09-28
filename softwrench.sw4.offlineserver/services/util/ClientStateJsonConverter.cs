using System;
using System.Collections.Generic;
using Iesi.Collections.Generic;
using Newtonsoft.Json.Linq;
using softwrench.sw4.batch.api.entities;
using softwrench.sw4.offlineserver.model.dto.association;

namespace softwrench.sw4.offlineserver.services.util {
    public class ClientStateJsonConverter {

        public class AppRowstampDTO {

            public string ApplicationName { get; set; }

            public IDictionary<string, string> ClientState  {
                get; set;
            }= new Dictionary<string, string>();
            public string MaxRowstamp {
                get; set;
            }
        }

        public static List<AppRowstampDTO> ConvertJSONToDict(JObject rowstampMap) {

            if (rowstampMap == null || !rowstampMap.HasValues) {
                return new List<AppRowstampDTO>();
            }

            var result = new List<AppRowstampDTO>();
            dynamic obj = rowstampMap;

            if (obj.applications != null) {
                foreach (dynamic app in obj.applications) {
                    var singleApp = ParseSingleApp(app.Value);
                    singleApp.ApplicationName = app.Name;
                    result.Add(singleApp);
                }
                return result;
            }
            return new List<AppRowstampDTO>() { ParseSingleApp(obj) };
        }

        private static AppRowstampDTO ParseSingleApp(dynamic obj) {
            var result = new AppRowstampDTO();
            if (obj is JValue) {
                result.MaxRowstamp = obj.Value?.ToString();
                result.ClientState = new Dictionary<string, string>();
                return result;
            }


            var resultDict = new Dictionary<string, string>();
            result.ClientState = resultDict;
            if (obj.maxrowstamp != null) {
                result.MaxRowstamp = obj.maxrowstamp;
            }

            if (obj.application != null) {
                result.ApplicationName = obj.application;
            }
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
