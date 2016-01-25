using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using cts.commons.web.Attributes;
using Iesi.Collections.Generic;
using log4net;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.services.submission;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.dto;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.util;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Security.Context;
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
        private readonly BatchRedirectionHelper _redirectionHelper;


        public FirstSolarWorkorderBatchController(FirstSolarWoValidationHelper validationHelper, BatchItemSubmissionService submissionService, BatchRedirectionHelper redirectionHelper) {
            _validationHelper = validationHelper;
            _submissionService = submissionService;
            _redirectionHelper = redirectionHelper;
            Log.Debug("init log...");
        }

        [HttpPost]
        public IApplicationResponse SubmitBatch(BatchSubmissionData batchData, [FromUri] FirstSolarBatchType batchType) {
            var batch = Batch.TransientInstance("workorder", SecurityFacade.CurrentUser());
            batch.Items = new HashedSet<BatchItem>(batchData.SpecificData.Select(s => FirstSolarDatamapConverterUtil.BuildBatchItem(s, batchData, batchType)).ToList());
            var resultBatch = _submissionService.Submit(batch, new BatchOptions() { Synchronous = true });
            var woDataSet = DataSetProvider.GetInstance().LookupDataSet("workorder", "list");
            var dto = _redirectionHelper.BuildDTO(resultBatch);
            var applicationListResult = woDataSet.GetList(
                MetadataProvider.Application("workorder").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("list")),
                dto);
            applicationListResult.SuccessMessage = batchType.GetSuccessMessage(resultBatch.TargetResults.Count);
            return applicationListResult;
        }

        [HttpPost]
        public IApplicationResponse InitBatch(BatchStartingData batchSharedData, [FromUri]FirstSolarBatchType batchType) {
            Log.DebugFormat("receiving batch data for {0}", batchType);
            var warningIds = _validationHelper.ValidateIdsThatHaveWorkorders(batchType, batchSharedData.Items, batchSharedData.Classification.Value);

            var i = 0;
            var resultData = batchSharedData.Items.Select(item => FirstSolarDatamapConverterUtil.DoGetDataMap(item, batchSharedData, warningIds, i++, batchType.GetUserIdName())).ToList();
            //assuring selected come first
            resultData.Sort(new SelectedComparer());

            var resultSchemaId = batchType.GetSpreadhSheetSchema();

            var schema = MetadataProvider.Application("workorder").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey(resultSchemaId)).Schema;

            return new ApplicationListResult(batchSharedData.Items.Count, null, resultData, schema, null) {
                ExtraParameters = new Dictionary<string, object>() { { "allworkorders", warningIds.Count == batchSharedData.Items.Count } }
            };
        }

        [HttpPost]
        public IGenericResponseResult ValidateExistingWorkorders([FromUri]string specificValue, [FromUri]string classificationId, [FromUri]FirstSolarBatchType batchType) {
            Log.DebugFormat("validating existing workorders for {0}", batchType);

            var itens = new List<MultiValueAssociationOption> {
                new MultiValueAssociationOption {
                    Value = specificValue,
                    Label = ""
                }
            };

            var result = new Dictionary<string, object>();
            var warningIds = _validationHelper.ValidateIdsThatHaveWorkorders(batchType, itens, classificationId);
            var hasWarning = warningIds.ContainsKey(specificValue);
            result["#warning"] = hasWarning;
            if (hasWarning) {
                result["#wonums"] = string.Join(",", warningIds[specificValue]);
            }

            return new GenericResponseResult<Dictionary<string, object>>(result);
        }

        [HttpGet]
        public IApplicationResponse GetListOfRelatedWorkorders(string location, string classification) {
            var listResult = _validationHelper.GetRelatedLocationWorkorders(location, classification);
            if (!listResult.ResultObject.Any()) {
                return new BlankApplicationResponse();
            }
            return listResult;
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
