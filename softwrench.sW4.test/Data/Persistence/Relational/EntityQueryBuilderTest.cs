using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using softwrench.sW4.TestBase;

namespace softwrench.sW4.test.Data.Persistence.Relational {
    [TestClass]
    public class EntityQueryBuilderTest : BaseMetadataTest{

        private readonly EntityQueryBuilder _builder = new EntityQueryBuilder();

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "test_only";
            MetadataProvider.StubReset();
        }

        [TestMethod]
        public void TestUnionOfCalls() {
            var wlMetadata = MetadataProvider.Entity("fakeentity");
            var dto = new PaginatedSearchRequestDto();
            dto.WhereClause = "3=3";
            dto.UnionWhereClauses = new List<string>{
                "1=1","2=2"
            };
            var result = _builder.AllRows(wlMetadata, dto);
            var resultST = "select fakeentity.fakeid as \"fakeid\", fakeentity.rowstamp as \"rowstamp\" from fakeentity as fakeentity  where (3=3) union all select fakeentity.fakeid as \"fakeid\", fakeentity.rowstamp as \"rowstamp\" from fakeentity as fakeentity  where (1=1) union all select fakeentity.fakeid as \"fakeid\", fakeentity.rowstamp as \"rowstamp\" from fakeentity as fakeentity  where (2=2) order by 1 desc";
            Assert.AreEqual(resultST, result.Sql);
        }
    }
}