using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using softwrench.sW4.TestBase;

namespace softwrench.sW4.test.Metadata.Applications.Command {
    [TestClass]
    public class UmcApplicationCommandCustomizationTest : BaseOtbMetadataTest {


        //test for SWWEB-3300 and SWWEB-3301 use cases
        [TestMethod]
        public void TestCommandCustomizationDetailActionBar() {
            var schema = MetadataProvider.Application("workorder").Schema(new ApplicationMetadataSchemaKey("editdetail"));
            var cs = schema.CommandSchema;
            Assert.IsTrue(cs.ApplicationCommands.ContainsKey("#actions"));
            var actions = cs.ApplicationCommands["#actions"];
            var originalCount = actions.Commands.Count;
            Assert.AreEqual(originalCount, 4);
            ApplicationConfiguration.TestclientName = "test4";
            MetadataProvider.StubReset();

            schema = MetadataProvider.Application("workorder").Schema(new ApplicationMetadataSchemaKey("editdetail"));
            cs = schema.CommandSchema;
            Assert.IsTrue(cs.ApplicationCommands.ContainsKey("#actions"));
            actions = cs.ApplicationCommands["#actions"];
            Assert.AreEqual(actions.Commands.Count, originalCount+1);

        }

        //test for SWWEB-3300 and SWWEB-3301 use cases
        [TestMethod]
        public void BarNotPresentInTemplateButInMetadataScenario() {

            ApplicationConfiguration.TestclientName = "test4";
            MetadataProvider.StubReset();

            var schema = MetadataProvider.Application("workorder").Schema(new ApplicationMetadataSchemaKey("editdetail"));
            var cs = schema.CommandSchema;
            Assert.IsTrue(cs.HasDeclaration);
            Assert.IsTrue(cs.ApplicationCommands.ContainsKey("#detail.primary"));
            var primaryBar = cs.ApplicationCommands["#detail.primary"];
            Assert.IsTrue(primaryBar.Commands.Any(f => f.Id.Equals("createWO")));
            Assert.IsTrue(primaryBar.Commands.Any(f => f.Id.Equals("save")));
        }



        [TestMethod]
        public void BarPresentInTemplateAndMetadataScenario() {
            ApplicationConfiguration.TestclientName = "firstsolar";
            MetadataProvider.StubReset();

            var schema = MetadataProvider.Application("fsocworkorder").Schema(new ApplicationMetadataSchemaKey("fsoceditdetail"));
            Assert.IsTrue(schema.CommandSchema.HasDeclaration);

            var actionsBar = schema.CommandSchema.ApplicationCommands["#actions"];
            Assert.IsTrue(actionsBar.Commands.Any(f => f.Id.Equals("createworkpackage")));
        }

       


    }
}
