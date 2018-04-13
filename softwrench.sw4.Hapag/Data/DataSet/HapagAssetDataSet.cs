using System;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Applications.DataSet.Filter;
using softwrench.sW4.Shared2.Data;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data;
using softWrench.sW4.Data.API;
using softWrench.sW4.Data.Entities.Historical;
using softWrench.sW4.Data.Persistence.SWDB;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Applications.DataSet;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Security.Services;
using softWrench.sW4.SimpleInjector;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using softwrench.sw4.Hapag.Data.DataSet.Helper;
using softwrench.sW4.Shared2.Util;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;

namespace softwrench.sw4.Hapag.Data.DataSet {
    class HapagAssetDataSet : BaseApplicationDataSet {

        private SWDBHibernateDAO _dao;

        private HapagImacDataSet _imacDataSet;

        //TODO: make these datasets injectables via SimpleInjector
        public HapagAssetDataSet() {

        }

        private HapagImacDataSet GetImacDataSet() {
            if (_imacDataSet == null) {
                _imacDataSet = (HapagImacDataSet)DataSetProvider.GetInstance().LookupAsBaseDataSet("imac");
            }
            return _imacDataSet;
        }

        private SWDBHibernateDAO GetDAO() {
            if (_dao == null) {
                _dao = SimpleInjectorGenericFactory.Instance.GetObject<SWDBHibernateDAO>(typeof(SWDBHibernateDAO));
            }
            return _dao;
        }

        protected override ApplicationListResult GetList(ApplicationMetadata application, softWrench.sW4.Data.Pagination.PaginatedSearchRequestDto searchDto) {
            var schemaId = application.Schema.SchemaId;
            if ("r0042ExportExcel" == schemaId) {
                return R0042Export(application, searchDto);
            }

            var dbList = base.GetList(application, searchDto);
            var resultObject = dbList.ResultObject;

            SchemaIdHandler(schemaId, resultObject);
            return dbList;
        }

        private ApplicationListResult R0042Export(ApplicationMetadata application, PaginatedSearchRequestDto dto) {
            var refDate = dto.SearchValues;
            var dates = refDate.Split('/'); // comes as 04/2018
            var newDto = new PaginatedSearchRequestDto { NeedsCountUpdate = false };
            var year = int.Parse(dates[1]);
            var month = int.Parse(dates[0]);

            var extractionDate = new DateTime(year, month, 1);

            newDto.AppendWhereClauseFormat("month(asset.changedate) = {0} and year(asset.changedate) = {1}", month, year);
            newDto.AppendSearchEntry("status", "!=DECOMMISSIONED");
            var dbList = base.GetList(application, newDto);

            if (!dbList.ResultObject.Any()) {
                return dbList;
            }

            var map = R0042QueryHelper.BuildIdMap(dbList);

            var assetIds = map.Keys.Select(s => s.AssetId).ToList();
            var assetNums = map.Keys.Select(s => s.AssetNum).ToList();

            var historicData = GetDAO().FindByNativeQuery(
                "select * from HIST_ASSETR0042 m inner join (select assetid,max(extractiondate) as max_date from HIST_ASSETR0042 where assetid in (:p0) and ExtractionDate <= :p1 group by assetid)i on (m.assetid = i.assetid and m.extractiondate = i.max_date) where m.assetid in (:p0) and m.extractiondate <= :p1", assetIds, extractionDate);


            var imacDataSet = GetImacDataSet();
            var imacs = imacDataSet.GetClosedImacIdsForR0042(assetNums, month,year);

            R0042QueryHelper.MergeImacData(map,imacs);
            R0042QueryHelper.MergeData(map, historicData);



            return dbList;
        }


        private static void SchemaIdHandler(string schemaId, IEnumerable<AttributeHolder> resultObject) {
            if (string.IsNullOrEmpty(schemaId)) {
                return;
            }
            if (schemaId == "assetlistreport") {
                HapagAssetListReportDataSet.FieldsHandler(resultObject);
            }
        }


        protected override ApplicationDetailResult GetApplicationDetail(ApplicationMetadata application, InMemoryUser user, DetailRequest request) {
            var dbDetail = base.GetApplicationDetail(application, user, request);
            var resultObject = dbDetail.ResultObject;
            if (resultObject != null && application.Schema.SchemaId == "detail" && application.Schema.Mode == SchemaMode.output) {
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

            var histTickets = resultObject.GetAttribute("histticket");

            // The workorder grid was replaced by IMAC grid

            var woData = GetDAO().FindByQuery<HistWorkorder>(HistWorkorder.ByAssetnum, assetId.ToString());
            foreach (var row in woData) {
                var list = (IList<Dictionary<string, object>>)resultObject.Attributes["imac_"];
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

            resultObject.Attributes["#assettree"] = new List<Dictionary<string, object>> { rootNode };
        }

        public SearchRequestDto AppendMultiLocciTicketHistoryQuery(CompositionPreFilterFunctionParameters preFilter) {
            var dto = preFilter.BASEDto;
            dto.SearchValues = null;
            dto.SearchParams = null;
            var assetNum = preFilter.OriginalEntity.GetAttribute("assetnum");
            dto.AppendWhereClauseFormat("ticketid in (select recordkey from MULTIASSETLOCCI multi where multi.assetnum = '{0}' and RECORDCLASS in ({1}) )", assetNum, "'CHANGE','INCIDENT','PROBLEM','SR'");

            return dto;
        }

        public SearchRequestDto AppendImacMultiLocciTicketHistoryQuery(CompositionPreFilterFunctionParameters preFilter) {
            var dto = preFilter.BASEDto;
            dto.SearchValues = null;
            dto.SearchParams = null;
            var assetNum = preFilter.OriginalEntity.GetAttribute("assetnum");
            //as of HAP-882
            dto.AppendWhereClauseFormat(@"ticketid in (select recordkey from MULTIASSETLOCCI multi where multi.assetnum = '{0}' and RECORDCLASS in ({1}) ) " +
                                        "or (imac.classificationid = '81515700' and imac.description = 'Decommission of {0}')", assetNum, "'CHANGE','INCIDENT','PROBLEM','SR'");
            dto.IgnoreWhereClause = true;
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
