using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications;

namespace softwrench.sw4.pae.classes.com.cts.pae.dataset {
    public class PaeAuditDataSet : SWDBApplicationDataset {

        private readonly IMaximoHibernateDAO _dao;

        public PaeAuditDataSet(IMaximoHibernateDAO dao) {
            _dao = dao;
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = await base.GetList(application, searchDto);

            await AddAssetDisplayInfo(result.ResultObject);

            return result;
        }

        private async Task AddAssetDisplayInfo(IEnumerable<AttributeHolder> entries) {
            // audit event entries for scanned assets
            var scanResults = entries
                .Where(entry => {
                    var action = entry.GetStringAttribute("action");
                    var refApplication = entry.GetStringAttribute("refApplication");
                    return action == "scan" && refApplication.EqualsAny("asset", "transportation");
                })
                .ToList();

            // no asset scans -> nothing to do
            if (!scanResults.Any()) {
                return;
            }

            // indexing result by assetid
            var indexedScanResults = new Dictionary<string, List<AttributeHolder>>();
            foreach (var entry in scanResults) {
                var assetId = entry.GetStringAttribute("refId");
                if (!indexedScanResults.ContainsKey(assetId)) {
                    indexedScanResults[assetId] = new List<AttributeHolder>();
                }
                indexedScanResults[assetId].Add(entry);
            }

            // get extra asset information to display 
            var assetInfo = await GetAssetInfo(indexedScanResults.Keys);

            if (assetInfo != null) {
                // set info in the result
                foreach (var asset in assetInfo) {
                    foreach (var entry in indexedScanResults[asset.AssetId]) {
                        entry["#asset_description"] = asset.Description;
                        entry["#asset_serialnum"] = asset.SerialNum;
                        entry["#asset_assetnum"] = entry["refUserId"];
                    }
                }
            }



        }

        private async Task<IEnumerable<AssetQueryResult>> GetAssetInfo(IEnumerable<string> assetIds) {
            var parameters = new ExpandoObject();
            var parametersCollection = ((ICollection<KeyValuePair<string, object>>)parameters);
            parametersCollection.Add(new KeyValuePair<string, object>("ids", assetIds.ToList()));

            var assetInfo = await _dao.FindByNativeQueryAsync("select assetid,description,serialnum from asset where assetid in (:ids)", parameters);

            return assetInfo.Cast<IDictionary<string, object>>().Select(AssetQueryResult.FromDictionary);
        }

        private class AssetQueryResult {
            public string AssetId {
                get; private set;
            }
            public string SerialNum {
                get; private set;
            }
            public string Description {
                get; private set;
            }
            public AssetQueryResult(string assetId, string serialNum, string description) {
                AssetId = assetId;
                SerialNum = serialNum;
                Description = description;
            }
            public static AssetQueryResult FromDictionary(IDictionary<string, object> dict) {
                var assetId = dict["assetid"].ToString();
                var serialNum = dict.ContainsKey("serialnum") ? dict["serialnum"] as string : null;
                var description = dict.ContainsKey("description") ? dict["description"] as string : null;
                return new AssetQueryResult(assetId, serialNum, description);
            }
        }

        public override string ApplicationName() {
            return "_audit";
        }

        public override string ClientFilter() {
            return "pae";
        }
    }
}