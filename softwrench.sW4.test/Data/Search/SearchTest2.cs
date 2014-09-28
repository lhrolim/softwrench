using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Search {
    [TestClass]
    public class SearchTest2 {

        private static ApplicationSchemaDefinition _schema;

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "otb";
            MetadataProvider.StubReset();
            var schemas = MetadataProvider.Application("invbalances").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("list", "input", "web")];
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
    }
}
