using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.TestBase;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Persistence.Relational.QueryBuilder.Basic {
    [TestClass]
    public class QuerySelectBuilderTest : BaseMetadataTest {
        [TestInitialize]
        public void Before() {
            if (!"hapag".Equals(ApplicationConfiguration.TestclientName)) {
                ApplicationConfiguration.TestclientName = "hapag";
                MetadataProvider.StubReset();
            }
        }


        [TestMethod]
        public void TestMethod1() {


            var dto = new SearchRequestDto();
            dto.AppendProjectionField(new ProjectionField("primaryuser_.personid", "primaryuser_.personid"));
            dto.AppendProjectionField(new ProjectionField("primaryuser_person_.displayname", "primaryuser_person_.displayname"));
            var result = QuerySelectBuilder.BuildSelectAttributesClause(MetadataProvider.Entity("asset"), QueryCacheKey.QueryMode.Detail, dto);
            Assert.IsTrue(result.Contains("primaryuser_.personid as primaryuser_personid, primaryuser_person_.displayname as primaryuser_person_displayname"));
        }


        [TestMethod]
        public void TestMethod2() {
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(new ProjectionField("value", "assetnum"));
            var result = QuerySelectBuilder.BuildSelectAttributesClause(MetadataProvider.Entity("asset"), QueryCacheKey.QueryMode.Detail, dto);
            Assert.IsTrue(result.Contains("asset.assetnum as value"));
        }


        [TestMethod]
        public void TestReplace() {
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(new ProjectionField("hlagdescription", "hlagdescription"));
            var result = QuerySelectBuilder.BuildSelectAttributesClause(MetadataProvider.Entity("asset"), QueryCacheKey.QueryMode.Detail, dto);
            Assert.IsTrue(result.Contains("CASE WHEN LOCATE('//',asset.Description) > 0 THEN LTRIM(RTRIM(SUBSTR(asset.Description, LOCATE('//', asset.Description)+3))) ELSE LTRIM(RTRIM(asset.Description)) END as hlagdescription"));
        }

        //test for adding null attributes, useful for schema union.
        //see sr.changeunionschema of hapag´s metadata.xml
        [TestMethod]
        public void TestReplace_Nulls() {
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(ProjectionField.Default("#null1"));
            dto.AppendProjectionField(ProjectionField.Default("#null2"));
            //unmappped
            dto.AppendProjectionField(ProjectionField.Default("#attr"));
            dto.AppendProjectionField(ProjectionField.Default("description"));
            var result = QuerySelectBuilder.BuildSelectAttributesClause(MetadataProvider.Entity("sr"), QueryCacheKey.QueryMode.List, dto);
            Assert.AreEqual(("select null, null, SR.description as description "), result);
        }

        [TestMethod]
        public void TestReplaceInner() {
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(ProjectionField.Default("primaryuser_.person_.hlagdisplayname"));
            var result = QuerySelectBuilder.BuildSelectAttributesClause(MetadataProvider.Entity("asset"), QueryCacheKey.QueryMode.Detail, dto);
            Assert.AreEqual(("select CASE WHEN LOCATE('@',primaryuser_person_.PERSONID) > 0 THEN '(' || SUBSTR(primaryuser_person_.PERSONID,1,LOCATE('@',primaryuser_person_.PERSONID)-1) || ') ' || COALESCE(primaryuser_person_.DISPLAYNAME,'-- Name Not Set --') ELSE '(' || primaryuser_person_.PERSONID || ') ' || COALESCE(primaryuser_person_.DISPLAYNAME,'-- Name Not Set --') END as primaryuser_person_hlagdisplayname "), result);
        }

        [TestMethod]
        public void TestProjectionFieldWithQuery() {
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(new ProjectionField("location", "DISTINCT SUBSTR(REPLACE(location.Location,'test',''),1,LOCATE('/',REPLACE(location.Location,'test',''))-1)"));
            var result = QuerySelectBuilder.BuildSelectAttributesClause(MetadataProvider.Entity("location"), QueryCacheKey.QueryMode.Detail, dto);
            Assert.AreEqual(("select DISTINCT SUBSTR(REPLACE(location.Location,'test',''),1,LOCATE('/',REPLACE(location.Location,'test',''))-1) as location "), result);
        }
    }
}
