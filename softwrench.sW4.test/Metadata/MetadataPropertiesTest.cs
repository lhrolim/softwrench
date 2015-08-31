﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata {
    [TestClass]
    public class MetadataPropertiesTest {

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "test_only";
            MetadataProvider.StubReset();
        }

        [TestMethod]
        public void ConfigThenEnvThenGlobal() {
            Assert.IsNull(MetadataProvider.GlobalProperty("wrong"));
            Assert.AreEqual(MetadataProvider.GlobalProperty("test"), "envvalue");
            Assert.AreEqual(MetadataProvider.GlobalProperty("global"), "globalvalue");
            Assert.AreEqual(MetadataProvider.GlobalProperty("config"), "configvalue");
        }

        [TestMethod]
        public void SchemaInheritFromApplicationProperties() {
            var locationApp = MetadataProvider.Application("location");
            var listSchema = locationApp.Schema(new ApplicationMetadataSchemaKey("list"));
            Assert.AreEqual(listSchema.GetProperty("mobile.fetchlimit"), "16000");
        }
    }
}
