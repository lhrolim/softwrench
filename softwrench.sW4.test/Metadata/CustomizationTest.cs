using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata;
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
            //parent fields=9; customizations=2
            Assert.AreEqual(11, displayables.Count);

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
            //parent fields=1 (auto-generated); customizations=1
            Assert.AreEqual(3, displayables.Count);

            Assert.IsNull(displayables.FirstOrDefault(f => f.Attribute.Equals("description")));

            Assert.IsNull(displayables.FirstOrDefault(f => f.Attribute.Equals("longdescription_.ldtext")));

            var zzzfield= displayables.FirstOrDefault(f => f.Attribute.Equals("zzz"));
            Assert.IsNotNull(zzzfield);

            var idx = displayables.IndexOf(zzzfield);
            Assert.AreEqual("www",displayables[idx+1].Attribute);

        }

    }
}
