using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata.Entities {

    [TestClass]
    public class ParentEntityTest : BaseMetadataTest {

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "hapag";
            MetadataProvider.StubReset();
        }

        [TestMethod]
        public void TestMethod1() {
            var imac=MetadataProvider.Entity("imac");
            var sr =MetadataProvider.Entity("sr");
            Assert.AreEqual(imac.GetTableName(),sr.GetTableName());
            Assert.AreNotEqual(imac.WhereClause, sr.WhereClause);
//            const EntityMetadata.AttributesMode includeCollections = EntityMetadata.AttributesMode.IncludeCollections;
//            Assert.AreEqual(imac.Attributes(includeCollections).Count(), sr.Attributes(includeCollections).Count());
//            Assert.AreEqual(imac.Associations.Count(), sr.Associations.Count());

        }
    }
}
