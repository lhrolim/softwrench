using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata {
    
    [TestClass]
    public class MissingSchemaFieldsTest {

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "test_only";
            MetadataProvider.StubReset();
        }

        [TestMethod]
        public void TestAddFk() {
            var appl = MetadataProvider.Application("attachment");
            var schema = appl.Schema(new ApplicationMetadataSchemaKey("list", SchemaMode.input, ClientPlatform.Mobile));
            Assert.IsNotNull(schema);
            var ownerIdField =schema.Fields.FirstOrDefault(f => f.Attribute == "ownerid");
            Assert.IsNotNull(ownerIdField);
            Assert.IsTrue(ownerIdField.IsHidden);

            var ownerTableField = schema.Fields.FirstOrDefault(f => f.Attribute == "ownertable");
            Assert.IsNotNull(ownerTableField);
            Assert.IsTrue(ownerTableField.IsHidden);

            schema = appl.Schema(new ApplicationMetadataSchemaKey(ApplicationMetadataConstants.SyncSchema, SchemaMode.input, ClientPlatform.Mobile));
            Assert.IsNotNull(schema);
            ownerIdField = schema.Fields.FirstOrDefault(f => f.Attribute == "ownerid");
            Assert.IsNotNull(ownerIdField);
            Assert.IsTrue(ownerIdField.IsHidden);

            ownerTableField = schema.Fields.FirstOrDefault(f => f.Attribute == "ownertable");
            Assert.IsNotNull(ownerTableField);
            Assert.IsTrue(ownerTableField.IsHidden);


            appl = MetadataProvider.Application("workorder");
            schema = appl.Schema(new ApplicationMetadataSchemaKey("editdetail", SchemaMode.input, ClientPlatform.Web));
            Assert.IsNotNull(schema);
            var crewIdField = schema.Fields.FirstOrDefault(f => f.Attribute == "crewid");
            Assert.IsNotNull(crewIdField);
            Assert.IsTrue(crewIdField.IsHidden);

            

        }
    }
}
