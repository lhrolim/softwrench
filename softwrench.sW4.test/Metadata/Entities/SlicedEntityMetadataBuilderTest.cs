﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
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
    public class SlicedEntityMetadataBuilderTest {
        
        private static ApplicationSchemaDefinition _schema;

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "hapag";
            MetadataProvider.StubReset();
            var schemas = MetadataProvider.Application("asset").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("detail", "output", "web")];
        }

        [TestMethod]
        public void TestFrom() {
            var sliced =SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("asset"),_schema);
            Assert.AreEqual(4,sliced.InnerMetadatas.Count);
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
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("srforchange"), schema,300,true);
            Assert.AreEqual(12, sliced.Attributes(EntityMetadata.AttributesMode.NoCollections).Count());
            var select = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.List);
            Debug.Write(select);
            Assert.IsTrue(select.Contains("null"));
        }
    }
}
