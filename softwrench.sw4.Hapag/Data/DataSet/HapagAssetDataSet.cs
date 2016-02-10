﻿using softWrench.sW4.Data.API.Response;
using softwrench.sW4.Shared2.Data;
﻿using softWrench.sW4.Data.Persistence.Dataset.Commons;
﻿using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Entities.Historical;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using cts.commons.simpleinjector;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagAssetDataSet : MaximoApplicationDataSet {

        private ISWDBHibernateDAO _dao;

        private HapagImacDataSet _imacDataSet;

        //TODO: make these datasets injectables via SimpleInjector
        public HapagAssetDataSet() {

        }

        private HapagImacDataSet GetImacDataSet() {
            if (_imacDataSet == null) {
                _imacDataSet = (HapagImacDataSet)DataSetProvider.GetInstance().LookupDataSet("imac", null);
            }
            return _imacDataSet;
        }

        private ISWDBHibernateDAO GetDAO() {
            if (_dao == null) {
                _dao = SimpleInjectorGenericFactory.Instance.GetObject<ISWDBHibernateDAO>(typeof(ISWDBHibernateDAO));
            }
            return _dao;
        }

        public override ApplicationListResult GetList(ApplicationMetadata application, softWrench.sW4.Data.Pagination.PaginatedSearchRequestDto searchDto) {
            var dbList = base.GetList(application, searchDto);
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


        public override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var dbDetail = base.GetApplicationDetail(application, user, request);
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
            resultObject.SetAttribute("#availableimacoptions", GetImacDataSet().GetAvailableImacsFromAsset(resultObject));
            var attribute = (List<Dictionary<string, object>>)resultObject.GetAttribute("assetcustodian_");
            resultObject.SetAttribute("#iscustodian", attribute.Any(a => a["personid"].Equals(SecurityFacade.CurrentUser(false).MaximoPersonId)));
        }

        private void FetchRemarks(DataMap resultObject) {
            var assetId = resultObject.GetAttribute("assetid");
            var extraAttributte = GetDAO().FindSingleByQuery<ExtraAttributes>(ExtraAttributes.ByMaximoTABLEIdAndAttribute, "asset", assetId.ToString(), "remarks");
            if (extraAttributte != null) {
                resultObject.SetAttribute(extraAttributte.AttributeName, extraAttributte.AttributeValue);
            }
        }

        private void JoinHistoryData(DataMap resultObject) {
            var assetId = resultObject.GetAttribute("itdextid");
            if (assetId == null) {
                return;
            }

            var woData = GetDAO().FindByQuery<HistWorkorder>(HistWorkorder.ByAssetnum, assetId.ToString());
            foreach (var row in woData) {
                var list = (IList<Dictionary<string, object>>)resultObject.Attributes["workorder_"];
                list.Add(row.toAttributeHolder());
            }

            var ticketData = GetDAO().FindByQuery<HistTicket>(HistTicket.ByAssetnum, assetId.ToString());
            var ticketList = (IList<Dictionary<string, object>>)resultObject.Attributes["ticket_"];
            var imacList = (IList<Dictionary<string, object>>)resultObject.Attributes["imac_"];
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
