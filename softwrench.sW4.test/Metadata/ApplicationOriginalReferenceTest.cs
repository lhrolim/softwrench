using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Util;
using softwrench.sW4.TestBase;

namespace softwrench.sW4.test.Metadata {
    [TestClass]
    public class ApplicationOriginalReferenceTest : BaseOtbMetadataTest {

        [TestMethod]
        public void TestReferenceWorkOrderList() {
            /* Temporarily used in TestAfterAndBefore1() Method */
            var app = MetadataProvider.Application("pastworkorder");
            var listSchema = app.Schema(new ApplicationMetadataSchemaKey("list",SchemaMode.None, ClientPlatform.Mobile));

            var originalApp = MetadataProvider.Application("workorder");
            var originalListSchema = originalApp.Schema(new ApplicationMetadataSchemaKey("list", SchemaMode.None, ClientPlatform.Mobile));

            Assert.IsTrue(listSchema.Displayables.Count > 1);
            Assert.IsNotNull(listSchema.Fields.FirstOrDefault(f=> f.Attribute.EqualsIc("workorderid")));
            Assert.IsNotNull(listSchema.Fields.FirstOrDefault(f=> f.Attribute.EqualsIc("wonum")));
            Assert.AreEqual(originalListSchema.Displayables.Count, listSchema.Displayables.Count);


        }


        public override string GetClientName() {
            return "firstsolar";
        }
    }
}
