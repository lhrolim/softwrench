using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata {
    [TestClass]
    public class CustomizationTest {


        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "test_only";
            MetadataProvider.StubReset();
        }


        [TestMethod]
        public void TestAfterAndBefore1() {
            var app = MetadataProvider.Application("worklog");
            var listSchema = app.Schema(new ApplicationMetadataSchemaKey("list"));
            var displayables = listSchema.Fields;
            //parent fields=8; customizations=2
            Assert.AreEqual(10, displayables.Count);

            var description = displayables.FirstOrDefault(f => f.Attribute.Equals("description"));
            var descIndex = displayables.IndexOf(description);

            Assert.AreEqual("xxx", displayables[descIndex+1].Attribute);

            var createdate = displayables.FirstOrDefault(f => f.Attribute.Equals("createdate"));
            var createdateIndex = displayables.IndexOf(createdate);

            Assert.AreEqual("yyy", displayables[createdateIndex-1].Attribute);
        }

        [TestMethod]
        public void TestReplace() {

            var app = MetadataProvider.Application("worklog");
            var detailSchema = app.Schema(new ApplicationMetadataSchemaKey("detail"));
            var displayables = detailSchema.Fields;
            //parent fields=3 (auto-generated); customizations=2
            Assert.AreEqual(5, displayables.Count);

            Assert.IsNull(displayables.FirstOrDefault(f => f.Attribute.Equals("description")));

            Assert.IsNull(displayables.FirstOrDefault(f => f.Attribute.Equals("longdescription_.ldtext")));

            var zzzfield= displayables.FirstOrDefault(f => f.Attribute.Equals("zzz"));
            Assert.IsNotNull(zzzfield);

            var idx = displayables.IndexOf(zzzfield);
            Assert.AreEqual("www",displayables[idx+1].Attribute);

        }

        [TestMethod]
        public void TestReplaceComposition() {

            var app = MetadataProvider.Application("incident");
            var detailSchema = app.Schema(new ApplicationMetadataSchemaKey("detail"));
            var compositions = detailSchema.Compositions;
            var attachmentComposition = compositions.FirstOrDefault(c => c.AssociationKey == "attachment");
            Assert.IsNull(attachmentComposition);

            var associations = detailSchema.Associations;
            Assert.IsNull(associations.FirstOrDefault(c => c.Attribute == "location"));
            //This was not replaced
            Assert.IsNotNull(associations.FirstOrDefault(c => c.Attribute == "ownergroup"));

            var optionFields =detailSchema.OptionFields;
            Assert.IsNull(optionFields.FirstOrDefault(c => c.Attribute == "classstructureid"));
        }

    }
}
