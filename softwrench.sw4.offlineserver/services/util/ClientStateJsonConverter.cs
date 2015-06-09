using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using softwrench.sw4.offlineserver.dto.association;

namespace softwrench.sw4.offlineserver.services.util {
    public class ClientStateJsonConverter {

        public static IDictionary<string, string> ConvertJSONToDict(JObject rowstampMap) {
            if (rowstampMap == null || !rowstampMap.HasValues) {
                return new Dictionary<string, string>();
            }
            var result = new Dictionary<string, string>();
            dynamic obj = rowstampMap;
            //Loop over the array
            foreach (dynamic row in obj.items) {
                var id = row.id;
                var rowstamp = row.rowstamp;
                if (id.Value != null) {
                    //new items, generated on the client side, do not need to be included
                    result.Add(id.Value, ""+rowstamp.Value);
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
                var maxRowstamp = row.Value.maximorowstamp.Value;
                //TODO: implement other fields
                result.Add(application, new ClientAssociationCacheEntry() {
                    MaxRowstamp = ""+ maxRowstamp
                });
            }
            return result;
        }
    }
}
