using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.web.Attributes;
using Iesi.Collections.Generic;
using log4net;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.util;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.dto;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.util;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SPF;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action {


    [Authorize]
    [SPFRedirect(URL = "Application")]
    [SWControllerConfiguration]
    public class FirstSolarWorkorderBatchController : ApiController {

        private static readonly ILog Log = LogManager.GetLogger(typeof(FirstSolarWorkorderBatchController));

        private readonly FirstSolarWoValidationHelper _validationHelper;
        private readonly BatchItemSubmissionService _submissionService;

        public FirstSolarWorkorderBatchController(FirstSolarWoValidationHelper validationHelper, BatchItemSubmissionService submissionService) {
            _validationHelper = validationHelper;
            _submissionService = submissionService;
            Log.Debug("init log...");
        }

        [HttpPost]
        public IApplicationResponse SubmitLocationBatch(LocationBatchSubmissionData batchData) {

            var batch = Batch.TransientInstance("workorder", SecurityFacade.CurrentUser());
            batch.Items = new HashedSet<BatchItem>(batchData.SpecificData.Select(l => FirstSolarDatamapConverterUtil.BuildBatchItem(l, batchData)).ToList());
            _submissionService.Submit(batch, new BatchOptions() { Synchronous = true });
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
            return FirstSolarDatamapConverterUtil.DoGetDataMap(location, batchData, warningIds, transientId, new Tuple<string, string>("location", "location_label"));
        }

        private DataMap GetAssetDataMap(IAssociationOption asset, BatchData batchData, IDictionary<string, List<string>> warningIds, int transientId) {
            return FirstSolarDatamapConverterUtil.DoGetDataMap(asset, batchData, warningIds, transientId, new Tuple<string, string>("assetnum", "asset_label"));
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



    }
}
