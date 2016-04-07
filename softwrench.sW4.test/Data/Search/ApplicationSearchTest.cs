using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using System;
using System.Collections.Generic;

namespace softwrench.sW4.test.Data.Search {
    [TestClass]
    public class ApplicationSearchTest {

        private ApplicationSchemaDefinition _schema;
        private ApplicationSchemaDefinition _assetSchema;

        [TestInitialize]
        public void Init() {
            if (ApplicationConfiguration.TestclientName != "hapag") {
                ApplicationConfiguration.TestclientName = "hapag";
                MetadataProvider.StubReset();
            }
            var schemas = MetadataProvider.Application("servicerequest").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("list", "input", "web")];
            var asset = MetadataProvider.Application("asset").Schemas();
            _assetSchema = asset[new ApplicationMetadataSchemaKey("list", "input", "web")];
        }

        [TestMethod]
        public void BasicSearchDtoTest() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "ticketid".Split(','), "teste");

            Assert.IsNotNull(searchRequestDto);
            Assert.IsTrue(searchRequestDto.SearchParams.Equals("ticketid"));
            Assert.IsTrue(searchRequestDto.SearchValues.Equals("teste"));

            String whereClause = SearchUtils.GetWhere(searchRequestDto, "SR");

            Assert.AreEqual("( SR.ticketid = :ticketid )", whereClause);
        }

        public void EmptySearchDtoTest() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "ticketid".Split(','), "");

            Assert.IsNotNull(searchRequestDto);
            Assert.IsTrue(searchRequestDto.SearchParams.Equals("ticketid"));
            Assert.IsTrue(searchRequestDto.SearchValues.Equals(""));

            var whereClause = SearchUtils.GetWhere(searchRequestDto, "SR");

            Assert.AreEqual("( UPPER(SR.ticketid) = :ticketid or SR.ticketid is null)", whereClause);
        }

        [TestMethod]
        public void MultipleFieldsSearchDtoTest() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "ticketid,description,asset_.description,siteid,affectedperson,status,reportdate".Split(','), "teste");

            Assert.IsNotNull(searchRequestDto);
            Assert.AreEqual("ticketid||,description||,asset_.description||,status", searchRequestDto.SearchParams);
            Assert.AreEqual("teste,,,teste,,,teste,,,teste", searchRequestDto.SearchValues);

            String whereClause = SearchUtils.GetWhere(searchRequestDto, "SR");

            Assert.AreEqual(@"( SR.ticketid = :ticketid ) OR ( SR.description = :description ) OR ( asset_.description = :asset_.description ) OR ( SR.status = :status )", whereClause);
        }

        [TestMethod]
        public void DateSearchDtoTest() {

            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "ticketid,description,asset_.description,siteid,affectedperson,status,reportdate".Split(','), "2013-01-01");

            Assert.IsNotNull(searchRequestDto);
            Assert.IsTrue(searchRequestDto.SearchParams.Equals("reportdate"));
            Assert.IsTrue(searchRequestDto.SearchValues.Equals("2013-01-01"));

            String whereClause = SearchUtils.GetWhere(searchRequestDto, "SR");

            Assert.IsTrue(whereClause.Equals("( SR.reportdate BETWEEN :reportdate_begin AND :reportdate_end )"));

            IDictionary<String, Object> parametersMap = SearchUtils.GetParameters(searchRequestDto);

            Assert.IsTrue(parametersMap.Count == 2);
            Assert.IsTrue(parametersMap["reportdate_begin"].Equals(DateUtil.BeginOfDay(DateTime.Parse("2013-01-01"))));
            Assert.IsTrue(parametersMap["reportdate_end"].Equals(DateUtil.EndOfDay(DateTime.Parse("2013-01-01"))));
        }

        [TestMethod]
        public void DateSearchDtoGteTest() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "ticketid,description,asset_.description,siteid,affectedperson,status,reportdate".Split(','), ">=2013-01-01");
            Assert.IsTrue(SearchUtils.GetWhere(searchRequestDto, "SR").Equals("( SR.reportdate >= :reportdate_begin )"));
            var where = SearchUtils.GetWhere(searchRequestDto, "sr", "sr");
            var parametersMap = SearchUtils.GetParameters(searchRequestDto);
            Assert.IsTrue(parametersMap.Count == 1);
            Assert.IsTrue(parametersMap["reportdate_begin"].Equals(DateUtil.BeginOfDay(DateTime.Parse("2013-01-01"))));
        }

        [TestMethod]
        public void DateTimeSearchDtoeqTest() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "reportdate".Split(','), "=2013-01-01 15:45");
            var @where = SearchUtils.GetWhere(searchRequestDto, "SR");
            Assert.AreEqual("( SR.reportdate BETWEEN :reportdate_begin AND :reportdate_end )", @where);
            var parametersMap = SearchUtils.GetParameters(searchRequestDto);
            Assert.IsTrue(parametersMap.Count == 2);
            Assert.IsTrue(parametersMap["reportdate_begin"].Equals(DateTime.Parse("2013-01-01 15:45:00")));
            Assert.IsTrue(parametersMap["reportdate_end"].Equals(DateTime.Parse("2013-01-01 15:45:59.999")));
        }


        [TestMethod]
        public void DateSearchDtoLTTest() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "ticketid,description,asset_.description,siteid,affectedperson,status,reportdate".Split(','), "<2013-01-01");
            Assert.IsTrue(SearchUtils.GetWhere(searchRequestDto, "SR").Equals("( SR.reportdate < :reportdate_end )"));
            var parametersMap = SearchUtils.GetParameters(searchRequestDto);
            Assert.IsTrue(parametersMap.Count == 1);
            Assert.IsTrue(parametersMap["reportdate_end"].Equals(DateUtil.EndOfDay(DateTime.Parse("2012-12-31"))));
        }

        [TestMethod]
        public void SearchWithQueryParameter() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_assetSchema, "computername".Split(','), "test%");
            Assert.AreEqual(("( UPPER(COALESCE(CASE WHEN LOCATE('//',asset.Description) > 0 THEN LTRIM(RTRIM(SUBSTR(asset.Description,1,LOCATE('//',asset.Description)-1))) ELSE LTRIM(RTRIM(asset.Description)) END,'')) like :computername )"), SearchUtils.GetWhere(searchRequestDto, "asset"));

        }

        [TestMethod]
        public void SearchWithFixedParam_Hapag_SEARCH_TEST() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "ticketid,description".Split(','), "%teste%");
            searchRequestDto.BuildFixedWhereClause("SR");
            var filterFixedWhereClause = searchRequestDto.FilterFixedWhereClause;
            Assert.AreEqual("( UPPER(COALESCE(SR.ticketid,'')) like '%TESTE%' ) OR ( UPPER(COALESCE(SR.description,'')) like '%TESTE%' )", filterFixedWhereClause);
        }
        [TestMethod]
        public void SearchWithFixedParam_Hapag_SEARCH_TEST_NOTEQ() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "ticketid".Split(','), "!=teste");
            searchRequestDto.BuildFixedWhereClause("SR");
            var filterFixedWhereClause = searchRequestDto.FilterFixedWhereClause;
            Assert.AreEqual("( UPPER(COALESCE(SR.ticketid,'')) != 'TESTE' OR SR.ticketid IS NULL  )", filterFixedWhereClause);
        }


        [TestMethod]
        public void NotEqEmptySearchDtoTest() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "ticketid".Split(','), "!=");

            Assert.IsNotNull(searchRequestDto);
            Assert.IsTrue(searchRequestDto.SearchParams.Equals("ticketid"));
            Assert.IsTrue(searchRequestDto.SearchValues.Equals("!="));

            var whereClause = SearchUtils.GetWhere(searchRequestDto, "SR");

            Assert.AreEqual("( UPPER(COALESCE(SR.ticketid,'')) != :ticketid OR SR.ticketid IS NULL  )", whereClause);
        }

        [TestMethod]
        public void NotEqNotEmptySearchDtoTest() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "ticketid".Split(','), "!=a");

            Assert.IsNotNull(searchRequestDto);
            Assert.IsTrue(searchRequestDto.SearchParams.Equals("ticketid"));
            Assert.IsTrue(searchRequestDto.SearchValues.Equals("!=a"));

            var whereClause = SearchUtils.GetWhere(searchRequestDto, "SR");

            Assert.AreEqual("( UPPER(COALESCE(SR.ticketid,'')) != :ticketid OR SR.ticketid IS NULL  )", whereClause);
        }
    }
}
