using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Persistence.Relational.QueryBuilder.Basic {
    [TestClass]
    public class QuerySelectBuilderTest {
        [ClassInitialize]
        public static void Before(TestContext testContext) {
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

        [TestMethod]
        public void ChangeSRUnionSearch() {
            var dto = new SearchRequestDto();
            dto.BuildUnionDTO(MetadataProvider.FindSchemaDefinition("change.list"));

            var completeOne = MetadataProvider.Application("change");
            var metadata = completeOne.ApplyPolicies(new ApplicationMetadataSchemaKey("list"), InMemoryUser.TestInstance(),
                ClientPlatform.Web);
            ApplicationSchemaDefinition schema;
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("wochange"), metadata.Schema,300);
            var result = QuerySelectBuilder.BuildSelectAttributesClause(sliced.UnionSchema,QueryCacheKey.QueryMode.Union, dto.unionDTO);
            Assert.AreEqual("select null, null, null, null, '-666' as zeroedattr, srforchange.description as hlagchangesummary, srforchange.ticketid as ticketid, asset_.description as asset_description, null, null, CASE WHEN srforchange.PLUSPCUSTOMER IS NOT NULL AND srforchange.PLUSPCUSTOMER = 'HLC-00' then 'HLC-00'  WHEN srforchange.PLUSPCUSTOMER IS NOT NULL AND LENGTH(srforchange.PLUSPCUSTOMER) >= 3 THEN SUBSTR(srforchange.PLUSPCUSTOMER, LENGTH(srforchange.PLUSPCUSTOMER) - 2, 3) ELSE '' END as hlagpluspcustomer, CASE WHEN LOCATE('@',srforchange.REPORTEDBY) > 0 THEN SUBSTR(srforchange.REPORTEDBY,1,LOCATE('@',srforchange.REPORTEDBY)-1) ELSE srforchange.REPORTEDBY END as hlagreportedby, srforchange.status as status ", result);
        }
    }
}
