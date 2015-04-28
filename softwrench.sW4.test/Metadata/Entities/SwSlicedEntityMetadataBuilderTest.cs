using cts.commons.portable.Util;
using cts.commons.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Util;
using System.Diagnostics;
using System.Linq;
using softwrench.sW4.batches.com.cts.softwrench.sw4.batches.entities;

namespace softwrench.sW4.test.Metadata.Entities {
    
    [TestClass]
    public class SwSlicedEntityMetadataBuilderTest {
        
        private static ApplicationSchemaDefinition _schema;

        private Batch batch;

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "otb";
            MetadataProvider.StubReset();
            var schemas = MetadataProvider.Application("_SoftwrenchError").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("detail", "input", "web")];
        }

        [TestMethod]
        public void TestSWDBSlicedEntity() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("_Problem"), _schema);
            Assert.AreNotEqual(sliced, null);
        }
        
    }
}
