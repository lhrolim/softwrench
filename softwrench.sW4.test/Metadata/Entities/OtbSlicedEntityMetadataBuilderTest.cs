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
    public class OtbSlicedEntityMetadataBuilderTest {
        
        private static ApplicationSchemaDefinition _schema;

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "otb";
            MetadataProvider.StubReset();
            var schemas = MetadataProvider.Application("incident").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("detail", "input", "web")];
        }

        [TestMethod]
        public void TestSelect() {
            var sliced =SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("incident"),_schema);
            var select = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.Detail);
            Debug.Write(select);
            Assert.IsFalse(select.Contains("person_.langcode"));
            Assert.IsTrue(select.Contains("person_.displayname"));
        }

        [TestMethod]
        public void TestFrom() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("incident"), _schema);
            var from = QueryFromBuilder.Build(sliced);
            Debug.Write(from);
            //assert that the relationships are not being duplicated
            Assert.AreEqual(1, from.GetNumberOfItems("solution_.solution"));
        }

      
    }
}
