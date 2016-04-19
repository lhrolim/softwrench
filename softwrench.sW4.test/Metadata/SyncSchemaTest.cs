using System.Linq;
using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Offline;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata {
    [TestClass]
    public class SyncSchemaTest : BaseMetadataTest {


        [TestInitialize]
        public void Init() {
            if (ApplicationConfiguration.TestclientName != "test_only") {
                ApplicationConfiguration.TestclientName = "test_only";
                MetadataProvider.StubReset();
            }
        }

        [TestMethod]
        public void TestMethod1() {
            var woApp = MetadataProvider.Application("workorder");
            var detailSchema = woApp.Schema(new ApplicationMetadataSchemaKey("detail", null, ClientPlatform.Mobile));
            var syncSchema = woApp.Schema(new ApplicationMetadataSchemaKey(ApplicationMetadataConstants.SyncSchema));
            Assert.IsTrue(syncSchema.Displayables.Count >= detailSchema.Displayables.Count);
            Assert.AreEqual(detailSchema.Compositions().Count, syncSchema.Compositions().Count);

        }

        [TestMethod]
        public void TestComposition() {
            var woApp = MetadataProvider.Application("worklog");
            var syncSchema = woApp.Schema(new ApplicationMetadataSchemaKey(ApplicationMetadataConstants.SyncSchema));
            Assert.IsTrue(syncSchema.Fields.Any(f => f.Attribute.EqualsIc(RowStampUtil.RowstampColumnName)));
        }
    }
}
