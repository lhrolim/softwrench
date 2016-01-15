using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using cts.commons.web.Attributes;
using JetBrains.Annotations;
using log4net;
using log4net.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.util;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action {


    [Authorize]
    [SPFRedirect(URL = "Application")]
    [SWControllerConfiguration]
    public class FirstSolarWorkorderBatchController : ApiController {

        private static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarWorkorderBatchController));

        private readonly FirstSolarWoValidationHelper _validationHelper;
        private readonly MultiItemBatchSubmissionService _submissionService;

        public FirstSolarWorkorderBatchController(FirstSolarWoValidationHelper validationHelper, MultiItemBatchSubmissionService submissionService) {
            _validationHelper = validationHelper;
            _submissionService = submissionService;
            Log.Debug("init log...");
        }

        [HttpPost]
        public IApplicationResponse SubmitLocationBatch(LocationBatchData batchData) {

            var datamapList = new List<DataMap>();
            batchData.Locations.ForEach(l => LocationBatchDataToWorkorderDataMep(l, batchData, datamapList));
            var jsonArray = JsonConvert.SerializeObject(datamapList);

            var userId = SecurityFacade.CurrentUser().DBId;
            var batch = new MultiItemBatch {
                Application = "workorder",
                Schema = "batchLocationSpreadSheet",
                UserId = userId,
                Status = BatchStatus.INPROG,
                CreationDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                DataMapJsonAsString = jsonArray
            };
            _submissionService.Submit(batch);
            return null;
        }


        [HttpGet]
        public IApplicationResponse GetListOfRelatedWorkorders(string location, string classification) {
            var listResult = _validationHelper.GetRelatedLocationWorkorders(location, classification);
            if (!listResult.ResultObject.Any()) {
                return new BlankApplicationResponse();
            }
            return listResult;
        }

        [HttpPost]
        public IApplicationResponse InitLocationBatch(LocationBatchData batchData) {

            Log.Debug("receiving batch data");
            var warningIds = _validationHelper.ValidateIdsThatHaveWorkordersForLocation(batchData.Locations, batchData.Classification);

            var i = 0;
            var resultData = batchData.Locations.Select(location => GetDataMap(location, batchData, warningIds, i++)).ToList();
            //assuring selected come first
            resultData.Sort(new SelectedComparer());

            var schema = MetadataProvider.Application("workorder").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("batchLocationSpreadSheet")).Schema;

            return new ApplicationListResult(batchData.Locations.Count, null, resultData, schema, null) {
                ExtraParameters = new Dictionary<string, object>() { { "allworkorders", warningIds.Count == batchData.Locations.Count } }
            };

        }

        [HttpPost]
        public IApplicationResponse InitAssetBatch(AssetBatchData batchData) {
            Log.Debug("receiving batch data");
            var warningIds = _validationHelper.ValidateIdsThatHaveWorkordersForAsset(batchData.Assets, batchData.Classification);

            var i = 0;
            var resultData = batchData.Assets.Select(asset => GetAssetDataMap(asset, batchData, warningIds, i++)).ToList();
            //assuring selected come first
            resultData.Sort(new SelectedComparer());

            var schema = MetadataProvider.Application("workorder").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("batchAssetSpreadSheet")).Schema;

            return new ApplicationListResult(batchData.Assets.Count, null, resultData, schema, null) {
                ExtraParameters = new Dictionary<string, object>() { { "allworkorders", warningIds.Count == batchData.Assets.Count } }
            };
        }

        private DataMap GetDataMap(IAssociationOption location, BatchData batchData, IDictionary<string, List<string>> warningIds, int transientId) {
            return DoGetDataMap(location, batchData, warningIds, transientId, new Tuple<string, string>("location", "location_label"));
        }

        private DataMap GetAssetDataMap(IAssociationOption asset, BatchData batchData, IDictionary<string, List<string>> warningIds, int transientId) {
            return DoGetDataMap(asset, batchData, warningIds, transientId, new Tuple<string, string>("assetnum", "asset_label"));
        }

        private DataMap DoGetDataMap(IAssociationOption item, BatchData batchData, IDictionary<string, List<string>> warningIds, int transientId, Tuple<string, string> fieldNames) {
            var selected = !warningIds.ContainsKey(item.Value);
            var fields = new Dictionary<string, object>();
            fields["_#selected"] = selected;
            if (!selected) {
                fields["#wonums"] = string.Join(",", warningIds[item.Value]);
            }
            //if not selected, let´s put a warning for the user
            fields["#warning"] = !selected;

            //this id is needed in order for the buffer to work properly
            fields["workorderid"] = transientId;

            fields["summary"] = batchData.Summary;
            fields["siteid"] = batchData.SiteId;
            fields["details"] = batchData.Details;
            fields[fieldNames.Item1] = item.Value;
            fields[fieldNames.Item2] = item.Label;
            return new DataMap("workorder", fields);
        }

        private void LocationBatchDataToWorkorderDataMep(AssociationOption location, LocationBatchData batchData,
            List<DataMap> datamapList) {
            var fields = new Dictionary<string, object>();
            fields["summary"] = batchData.Summary;
            fields["siteid"] = batchData.SiteId;
            fields["details"] = batchData.Details;
            fields["location"] = location.Value;
            var datamap = new DataMap("workorder", fields);
            datamapList.Add(datamap);
        }

        private class SelectedComparer : IComparer<DataMap> {
            public int Compare(DataMap x, DataMap y) {

                var firstSelected = (bool)x.GetAttribute("_#selected");
                var secondSelected = (bool)y.GetAttribute("_#selected");
                if (firstSelected && !secondSelected) {
                    return -1;
                }
                if (firstSelected && secondSelected) {
                    return 0;
                }
                if (!firstSelected && secondSelected) {
                    return 1;
                }
                return 0;
            }
        }

        public class BatchData {

            public string Summary {
                get; set;
            }
            public string Details {
                get; set;
            }
            public string SiteId {
                get; set;
            }

            public string Classification {
                get; set;
            }


        }

        public class LocationBatchData : BatchData {
            public List<AssociationOption> Locations {
                get; set;
            }
        }

        public class AssetBatchData : BatchData {
            public List<AssociationOption> Assets {
                get; set;
            }
        }

        public class AssetBatchSubmissionData {

            public BatchData SharedData {
                get; set;
            }

            public IDictionary<string, BatchData> LocationSpecificData {
                get; set;
            }
        }

    }
}
