using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.TestBase;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Util;
using System.Diagnostics;
using System.Linq;

namespace softwrench.sW4.test.Metadata.Entities {

    [TestClass]
    public class HapagSlicedEntityMetadataBuilderTest : BaseMetadataTest {

        private static ApplicationSchemaDefinition _schema;

        [TestInitialize]
        public void Init() {
            ApplicationConfiguration.TestclientName = "hapag";
            MetadataProvider.StubReset();
            var schemas = MetadataProvider.Application("asset").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("detail", "output", "web")];
        }

        [TestMethod]
        public void TestFrom() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("asset"), _schema);
            Assert.AreEqual(4, sliced.InnerMetadatas.Count);
            var from = QueryFromBuilder.Build(sliced);
            Debug.Write(from);
            Assert.IsTrue(from.Contains("address as location_shipto_"));
            Assert.IsTrue(from.Contains("address as location_billto_"));
            Assert.IsTrue(from.Contains("address as location_serv_"));
            Assert.IsTrue(from.Contains("on (location_.billtoaddresscode"));
            Assert.IsTrue(from.Contains("on (location_.shiptoaddresscode"));
            Assert.IsTrue(from.Contains("on (location_.serviceaddresscode"));
        }

        [TestMethod]
        public void TestSelect() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("asset"), _schema);
            var select = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.Detail);
            Debug.Write(select);
            Assert.IsFalse(select.Contains("parentasset_.eq24"));
            Assert.IsTrue(select.Contains("location_shipto_.address1 as \"location_shipto_.address1\""));
            Assert.IsTrue(select.Contains("location_billto_.address1 as \"location_billto_.address1\""));
            Assert.IsTrue(select.Contains("location_serv_.address1 as \"location_serv_.address1\""));
        }



        [TestMethod]
        public void TestSelect2() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("asset"), _schema);
            var select = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.List);
            Debug.Write(select);
            Assert.IsTrue(select.Contains("location_shipto_.address1 as \"location_shipto_.address1\""));
            Assert.IsTrue(select.Contains("location_billto_.address1 as \"location_billto_.address1\""));
            Assert.IsTrue(select.Contains("location_serv_.address1 as \"location_serv_.address1\""));
        }

        /// <summary>
        /// this test assures that any second level attributes are not being fetched without the proper need.
        /// 28/07/2014: only sr_asset_.serialnum is asked for
        /// 
        /// </summary>
        [TestMethod]
        public void TestSelect3() {
            var schemas = MetadataProvider.Application("change").Schemas();
            var schema = schemas[new ApplicationMetadataSchemaKey("detail", null, "web")];
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("wochange"), schema);
            var select = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.Detail);
            Debug.Write(select);
            Assert.IsFalse(select.Contains("sr_asset_.installdate"));
            Assert.IsTrue(select.Contains("sr_asset_.serialnum"));
        }

        [TestMethod]
        public void TestSelect4() {
            var schemas = MetadataProvider.Application("imac").Schemas();
            var schema = schemas[new ApplicationMetadataSchemaKey("detail", "output", "web")];
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("imac"), schema);
            var select = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.Detail);
            Debug.Write(select);
            Assert.IsFalse(select.Contains("case when imac.serialnum"));
            Assert.IsTrue(select.Contains("case when asset_.serialnum"));
        }

        [TestMethod]
        public void TestSelectNullAttributes() {
            var schemas = MetadataProvider.Application("srforchange").Schemas();
            var schema = schemas[new ApplicationMetadataSchemaKey("changeunionschema", null, "web")];
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("srforchange"), schema, 300, true);
//            Assert.AreEqual(12, sliced.Attributes(EntityMetadata.AttributesMode.NoCollections).Count());
            var select = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.List);
            Debug.Write(select);
            Assert.IsTrue(select.Contains("null"));
        }

        /// <summary>
        /// userId and currentITc were blank due to a bug on the framework
        /// </summary>
        [TestMethod]
        public void TestAssetListReportBug() {
            var schemas = MetadataProvider.Application("asset").Schemas();
            var schema = schemas[new ApplicationMetadataSchemaKey("assetlistreport", null, "web")];
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("asset"), schema);
            var select = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.List);
            Debug.Write(select);
            Assert.IsTrue(select.Contains("CASE WHEN LOCATE('@',aucisowner_.PERSONID) > 0 THEN SUBSTR(aucisowner_.PERSONID,1,LOCATE('@',aucisowner_.PERSONID)-1) ELSE aucisowner_.PERSONID END as \"aucisowner_.hlagdisplayname\""));
        }

        [TestMethod]
        public void TestReverseMapping() {
            if (!"manchester".Equals(ApplicationConfiguration.TestclientName)) {
                ApplicationConfiguration.TestclientName = "manchester";
                MetadataProvider.StubReset();
            }

            var schemas = MetadataProvider.Application("servicerequest").Schemas();
            var schema = schemas[new ApplicationMetadataSchemaKey("editdetail", null, "web")];
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("sr"), schema, 300, true);
            var result = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.Detail);
            Debug.Write(result);

            //should not cointain the reverse associations here
            Assert.IsFalse(result.Contains("tkserviceaddress"));
            var from = QueryFromBuilder.Build(sliced);
            Debug.Write(from);
            Assert.IsFalse(from.Contains("tkserviceaddress"));
        }
    }
}
