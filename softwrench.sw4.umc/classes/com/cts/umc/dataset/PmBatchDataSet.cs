using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using softwrench.sw4.batch.api.entities;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.API.Composition;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sw4.umc.classes.com.cts.umc.dataset {

    public class PmBatchDataSet : SWDBApplicationDataset {

        private readonly IContextLookuper _contextLookuper;
        private readonly ISWDBHibernateDAO _dao;

        public PmBatchDataSet(IContextLookuper contextLookuper, ISWDBHibernateDAO dao) {
            _contextLookuper = contextLookuper;
            _dao = dao;
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, PaginatedSearchRequestDto searchDto) {
            var result = await base.GetList(application, searchDto);
            foreach (var item in result.ResultObject) {
                var status = item.GetAttribute("status") as string;
                if (BatchStatus.SUBMITTING.ToString().EqualsIc(status)) {
                    var id = item.GetAttribute("id");
                    var report = _contextLookuper.GetFromMemoryContext<BatchReport>("sw_batchreport{0}".Fmt(id));
                    if (report != null) {
                        item.SetAttribute("status", "Submitting {0} %".Fmt(report.PercentageDone));
                    }
                }

            }
            return result;
        }

        protected override async Task<DataMap> FetchDetailDataMap(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var datamap = await base.FetchDetailDataMap(application, user, request);
            var batch = await _dao.FindByPKAsync<MultiItemBatch>(int.Parse(request.Id));

            if (BatchStatus.SUBMITTING.Equals(batch.Status)) {
                var report = _contextLookuper.GetFromMemoryContext<BatchReport>("sw_batchreport{0}".Fmt(batch.Id));
                if (report != null) {
                    datamap["status"] = "Submitting {0} %".Fmt(report.PercentageDone);
                }
            }

            datamap.Add("ld", GetLd(batch));
            return datamap;
        }

        public override async Task<CompositionFetchResult> GetCompositionData(ApplicationMetadata application, CompositionFetchRequest request, JObject currentData) {
            var result = await base.GetCompositionData(application, request, currentData);
            var batch = await _dao.FindByPKAsync<MultiItemBatch>(int.Parse(request.Id));
            var report = _contextLookuper.GetFromMemoryContext<BatchReport>("sw_batchreport{0}".Fmt(request.Id));

            // searches the pms of the batch
            var reqPag = request.PaginatedSearch;
            var total = batch.ItemIds?.Split(',').Length ?? 0;
            var pagination = new PaginatedSearchRequestDto(total, reqPag.PageNumber, reqPag.PageSize, null, reqPag.PaginationOptions) {
                SearchSort = "pmnum",
                SearchAscending = true
            };
            pagination.AppendWhereClause($" pm.pmuid in ({batch.ItemIds}) ");
            var user = SecurityFacade.CurrentUser();
            var pmApp = MetadataProvider.Application("pm").ApplyPolicies(new ApplicationMetadataSchemaKey("pmsimplelist"), user, ClientPlatform.Web);
            var searchResut = await GetList(pmApp, pagination);

            // add the updated value
            var pmList = new List<Dictionary<string, object>>();
            var processed = new List<string>();
            if (!BatchStatus.COMPLETE.Equals(batch.Status)) {
                var calcProcessed = report?.SentItemIds?.Split(',').ToList();
                if (calcProcessed != null) {
                    processed = calcProcessed;
                }
            }
            searchResut.ResultObject.ForEach(row => {
                row["updated"] = BatchStatus.COMPLETE.Equals(batch.Status) || processed.Contains(row["pmuid"].ToString());
                pmList.Add(row);
            });

            // adds result to composition response
            var pms = new EntityRepository.SearchEntityResult {
                IdFieldName = "pmuid",
                ResultList = pmList,
                PaginationData = new PaginatedSearchRequestDto(total, reqPag.PageNumber, reqPag.PageSize, null, reqPag.PaginationOptions)
            };
            result.ResultObject.Add("#pmupdated_", pms);

            return result;
        }

        private static string GetLd(MultiItemBatch batch) {
            var jsonString = batch.DataMapJsonAsString;
            var root = JObject.Parse(jsonString);
            if (root == null) return null;

            JToken pmsToken;
            if (!root.TryGetValue("#pmlist_", out pmsToken)) return null;

            var pms = pmsToken as JArray;

            var jobject = pms?.First as JObject;
            return jobject?.StringValue("DESCRIPTION_LONGDESCRIPTION");
        }

        public override string ApplicationName() {
            return "_pmbatch";
        }

    }
}
