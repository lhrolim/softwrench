using softWrench.sW4.Data.API.Response;
using softwrench.sW4.Shared2.Data;
﻿using softWrench.sW4.Data.Persistence.Dataset.Commons;
﻿using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Entities.Historical;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.persistence;

namespace softwrench.sw4.Hapag.Data.DataSet {
    public class HapagAssetDataSet : MaximoApplicationDataSet {

        private readonly ISWDBHibernateDAO _dao;
        private readonly HapagImacDataSet _imacDataSet;

        public HapagAssetDataSet(ISWDBHibernateDAO dao, HapagImacDataSet imacDataSet)
        {
            _dao = dao;
            _imacDataSet = imacDataSet;
        }

        public override async Task<ApplicationListResult> GetList(ApplicationMetadata application, softWrench.sW4.Data.Pagination.PaginatedSearchRequestDto searchDto) {
            var dbList = await base.GetList(application, searchDto);
            var resultObject = dbList.ResultObject;
            SchemaIdHandler(application.Schema.SchemaId, resultObject);
            return dbList;
        }

        private static void SchemaIdHandler(string schemaId, IEnumerable<AttributeHolder> resultObject) {
            if (string.IsNullOrEmpty(schemaId)) {
                return;
            }
            if (schemaId == "exportallthecolumns") {
                HapagAssetListReportDataSet.FieldsHandler(resultObject);
            }
        }


        public override async Task<ApplicationDetailResult> GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var dbDetail = await base.GetApplicationDetail(application, user, request);
            var resultObject = dbDetail.ResultObject;
            if (application.Schema.SchemaId == "detail" && application.Schema.Mode == SchemaMode.output) {
                HandleAssetTree(resultObject);
                FetchRemarks(resultObject);
                JoinHistoryData(resultObject);
                FillCreationFromAssetData(resultObject);
            }
            return dbDetail;
        }

        private void FillCreationFromAssetData(DataMap resultObject) {
            resultObject.SetAttribute("#availableimacoptions", _imacDataSet.GetAvailableImacsFromAsset(resultObject));
            var attribute = (List<Dictionary<string, object>>)resultObject.GetAttribute("assetcustodian_");
            resultObject.SetAttribute("#iscustodian", attribute.Any(a => a["personid"].Equals(SecurityFacade.CurrentUser(false).MaximoPersonId)));
        }

        private void FetchRemarks(DataMap resultObject) {
            var assetId = resultObject.GetAttribute("assetid");
            var extraAttributte = _dao.FindSingleByQuery<ExtraAttributes>(ExtraAttributes.ByMaximoTABLEIdAndAttribute, "asset", assetId.ToString(), "remarks");
            if (extraAttributte != null) {
                resultObject.SetAttribute(extraAttributte.AttributeName, extraAttributte.AttributeValue);
            }
        }

        private void JoinHistoryData(DataMap resultObject) {
            var assetId = resultObject.GetAttribute("itdextid");
            if (assetId == null) {
                return;
            }

            var woData = _dao.FindByQuery<HistWorkorder>(HistWorkorder.ByAssetnum, assetId.ToString());
            foreach (var row in woData) {
                var list = (IList<Dictionary<string, object>>)resultObject["workorder_"];
                list.Add(row.toAttributeHolder());
            }

            var ticketData = _dao.FindByQuery<HistTicket>(HistTicket.ByAssetnum, assetId.ToString());
            var ticketList = (IList<Dictionary<string, object>>)resultObject["ticket_"];
            var imacList = (IList<Dictionary<string, object>>)resultObject["imac_"];
            foreach (var row in ticketData) {
                var attributeHolder = row.toAttributeHolder();
                if (row.Classification != null && row.Classification.StartsWith("8151")) {
                    imacList.Add(attributeHolder);
                } else {
                    ticketList.Add(attributeHolder);
                }
            }
        }

        private void HandleAssetTree(DataMap resultObject) {

            var childs = ((IEnumerable<Dictionary<string, object>>)resultObject.GetAttribute("childassets_")).ToList();

            foreach (var item in childs) {
                item["label"] = item[("hlagdescription2")];
                item["children"] = new List<Dictionary<string, object>>();
            }

            var currentAsset = new Dictionary<string, object>();
            currentAsset["assetid"] = resultObject.GetAttribute("assetid");
            currentAsset["label"] = resultObject.GetAttribute("hlagdescription2");
            currentAsset["selected"] = "selected";
            currentAsset["children"] = childs;
            var rootNode = currentAsset;

            if (resultObject.GetAttribute("parentasset_.assetid") != null) {
                var parent = new Dictionary<string, object>();
                parent["assetid"] = resultObject.GetAttribute("parentasset_.assetid");
                parent["label"] = resultObject.GetAttribute("parentasset_.hlagdescription2");
                parent["children"] = new List<Dictionary<string, object>>() { currentAsset };
                rootNode = parent;
            }
        }

        public SearchRequestDto AppendMultiLocciTicketHistoryQuery(CompositionPreFilterFunctionParameters preFilter) {
            var dto = preFilter.BASEDto;
            dto.SearchValues = null;
            dto.SearchParams = null;
            var assetNum = preFilter.OriginalEntity.GetAttribute("assetnum");
            dto.AppendWhereClauseFormat("ticketid in (select recordkey from MULTIASSETLOCCI multi where multi.assetnum = '{0}' and RECORDCLASS in ({1}) )", assetNum, "'CHANGE','INCIDENT','PROBLEM','SR'");
            return dto;
        }

        public override string ApplicationName() {
            return "asset";
        }

        public override string ClientFilter() {
            return "hapag";
        }
    }
}
