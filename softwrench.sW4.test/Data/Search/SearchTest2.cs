using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Search {
    [TestClass]
    public class SearchTest2 {

        private ApplicationSchemaDefinition _schema;

        [TestInitialize]
        public void Init() {
            if (ApplicationConfiguration.TestclientName != "otb") {
                ApplicationConfiguration.TestclientName = "otb";
                MetadataProvider.StubReset();
            }
            var schemas = MetadataProvider.Application("invbalances").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("invbalancesList", "input", "web")];
        }

        [TestMethod]
        public void NumberSearchDtoGteTest() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "curbal".Split(','), ">=20");
            Assert.AreEqual("( invbalances.curbal >= :curbal )", SearchUtils.GetWhere(searchRequestDto, "invbalances"));
            var parametersMap = SearchUtils.GetParameters(searchRequestDto);
            Assert.IsTrue(parametersMap.Count == 1);
            Assert.IsTrue(parametersMap["curbal"].Equals(20));
        }

        [TestMethod]
        public void NumberBlankTest() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.SetFromSearchString(_schema, "curbal".Split(','), "!@BLANK");
            Assert.AreEqual("( invbalances.curbal IS NULL )", SearchUtils.GetWhere(searchRequestDto, "invbalances"));
            var parametersMap = SearchUtils.GetParameters(searchRequestDto);
            Assert.AreEqual(0, parametersMap.Count);
        }

        [TestMethod]
        public void AllowNullTest() {
            var searchRequestDto = new PaginatedSearchRequestDto(100, PaginatedSearchRequestDto.DefaultPaginationOptions);
            searchRequestDto.AppendSearchEntry("curbal", "20", true);
            Assert.AreEqual("( invbalances.curbal = :curbal OR invbalances.curbal IS NULL  )", SearchUtils.GetWhere(searchRequestDto, "invbalances"));
            var parametersMap = SearchUtils.GetParameters(searchRequestDto);
            Assert.IsTrue(parametersMap.Count == 1);
            Assert.IsTrue(parametersMap["curbal"].Equals(20));
        }
    }
}
