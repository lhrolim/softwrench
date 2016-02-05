using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.batchapi.com.cts.softwrench.sw4.batches.api.entities;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;

namespace softwrench.sW4.test.Metadata.Entities {
    
    [TestClass]
    public class SwSlicedEntityMetadataBuilderTest {
        
        private static ApplicationSchemaDefinition _schema;

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "otb";
            MetadataProvider.StubReset();
            var schemas = MetadataProvider.Application("_SoftwrenchError").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("detail", "input", "web")];
        }

        [TestMethod]
        public void TestSWDBSlicedEntity() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("Problem_"), _schema);
            IApplicationDisplayable rowstamp = new ApplicationFieldDefinition("_SoftwrenchError", "rowstamp", "rowstamp");
            Assert.IsFalse(sliced.AppSchema.Displayables.Contains(rowstamp));
            Assert.IsFalse(sliced.AppSchema.Displayables.Contains(null));
            Assert.AreNotEqual(sliced, null);
        }
        
    }
}
