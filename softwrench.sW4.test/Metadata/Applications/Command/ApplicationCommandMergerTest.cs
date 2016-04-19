using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Command;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata.Applications.Command {
    [TestClass]
    public class ApplicationCommandMergerTest : BaseMetadataTest {
        private readonly CommandBarDefinition _commandBarDefinition = new CommandBarDefinition(null, "detail", false, new List<ApplicationCommand>
            {
                ApplicationCommand.TestInstance("c1","","label","icon"),
                ApplicationCommand.TestInstance("c2"),
                ApplicationCommand.TestInstance("c3"),
            });

        [TestMethod]
        public void TestEmptySchema() {
            var bars = new Dictionary<string, CommandBarDefinition>();
            bars["detail"] = _commandBarDefinition;
            var result = ApplicationCommandMerger.MergeCommands(new Dictionary<string, CommandBarDefinition>(), bars);
            Assert.IsFalse(result.Keys.Any());
        }

        [TestMethod]
        public void TestRemoveCommand() {
            var bars = new Dictionary<string, CommandBarDefinition>();
            bars["detail"] = _commandBarDefinition;
            var commandBarDefinitions = new Dictionary<string, CommandBarDefinition>();
            commandBarDefinitions["detail"] = new CommandBarDefinition(null, "detail", false, new List<RemoveCommand>
            {
                new RemoveCommand("c2")
            });
            var result = ApplicationCommandMerger.MergeCommands(commandBarDefinitions, bars);
            Assert.IsTrue(result.Keys.Any());
            Assert.AreEqual(1, result.Keys.Count);
            var commandBarDefinition = result["detail"];
            Assert.AreEqual(2, commandBarDefinition.Commands.Count());
            Assert.AreEqual("c1", commandBarDefinition.Commands[0].Id);
            Assert.AreEqual("c3", commandBarDefinition.Commands[1].Id);
        }

        [TestMethod]
        public void TestAddCommandRightTo() {
            var bars = new Dictionary<string, CommandBarDefinition>();
            bars["detail"] = _commandBarDefinition;
            var commandBarDefinitions = new Dictionary<string, CommandBarDefinition>();
            commandBarDefinitions["detail"] = new CommandBarDefinition(null, "detail", false, new List<ApplicationCommand>
            {
                ApplicationCommand.TestInstance("c4",">c2")
            });
            var result = ApplicationCommandMerger.MergeCommands(commandBarDefinitions, bars);
            Assert.IsTrue(result.Keys.Any());
            Assert.AreEqual(1, result.Keys.Count);
            var commandBarDefinition = result["detail"];
            Assert.AreEqual(4, commandBarDefinition.Commands.Count());
            Assert.AreEqual("c1", commandBarDefinition.Commands[0].Id);
            Assert.AreEqual("c2", commandBarDefinition.Commands[1].Id);
            Assert.AreEqual("c4", commandBarDefinition.Commands[2].Id);
            Assert.AreEqual("c3", commandBarDefinition.Commands[3].Id);
        }

        [TestMethod]
        public void TestAddCommandRightTo2() {
            var bars = new Dictionary<string, CommandBarDefinition>();
            bars["detail"] = _commandBarDefinition;
            var commandBarDefinitions = new Dictionary<string, CommandBarDefinition>();
            commandBarDefinitions["detail"] = new CommandBarDefinition(null, "detail", false, new List<ICommandDisplayable>
            {
                new ResourceCommand("c4","b","",">c3",null)
            });
            var result = ApplicationCommandMerger.MergeCommands(commandBarDefinitions, bars);
            Assert.IsTrue(result.Keys.Any());
            Assert.AreEqual(1, result.Keys.Count);
            var commandBarDefinition = result["detail"];
            Assert.AreEqual(4, commandBarDefinition.Commands.Count());
            Assert.AreEqual("c1", commandBarDefinition.Commands[0].Id);
            Assert.AreEqual("c2", commandBarDefinition.Commands[1].Id);
            Assert.AreEqual("c3", commandBarDefinition.Commands[2].Id);
            Assert.AreEqual("c4", commandBarDefinition.Commands[3].Id);
        }

        [TestMethod]
        public void TestReplaceCommandKeepingOriginals() {
            var bars = new Dictionary<string, CommandBarDefinition>();
            bars["detail"] = _commandBarDefinition;
            var commandBarDefinitions = new Dictionary<string, CommandBarDefinition>();
            commandBarDefinitions["detail"] = new CommandBarDefinition(null, "detail", false, new List<ApplicationCommand>
            {
                ApplicationCommand.TestInstance("c1",null,"testchangelabel")
            });
            var result = ApplicationCommandMerger.MergeCommands(commandBarDefinitions, bars);
            Assert.IsTrue(result.Keys.Any());
            Assert.AreEqual(1, result.Keys.Count);
            var commandBarDefinition = result["detail"];
            Assert.AreEqual(3, commandBarDefinition.Commands.Count());
            var commandDisplayable = (ApplicationCommand)commandBarDefinition.Commands[0];
            Assert.AreEqual("c1", commandDisplayable.Id);
            Assert.AreEqual("testchangelabel", commandDisplayable.Label);
            Assert.AreEqual("icon", commandDisplayable.Icon);
            Assert.AreEqual("c2", commandBarDefinition.Commands[1].Id);
            Assert.AreEqual("c3", commandBarDefinition.Commands[2].Id);
        }

        [TestMethod]
        public void TestRemoveUndeclared() {
            var bars = new Dictionary<string, CommandBarDefinition>();
            bars["detail"] = _commandBarDefinition;
            var commandBarDefinitions = new Dictionary<string, CommandBarDefinition>();
            commandBarDefinitions["detail"] = new CommandBarDefinition(null, "detail", true, new List<ApplicationCommand>
            {
                ApplicationCommand.TestInstance("c4",null)
            });
            var result = ApplicationCommandMerger.MergeCommands(commandBarDefinitions, bars);
            Assert.IsTrue(result.Keys.Any());
            Assert.AreEqual(1, result.Keys.Count);
            var commandBarDefinition = result["detail"];
            Assert.AreEqual(1, commandBarDefinition.Commands.Count());
            var commandDisplayable = (ApplicationCommand)commandBarDefinition.Commands[0];
            Assert.AreEqual("c4", commandDisplayable.Id);
        }


        [TestMethod]
        public void TestMergeWithParent() {
            ApplicationConfiguration.TestclientName = "otb";
            MetadataProvider.StubReset();
            var schema = MetadataProvider.Application("person").Schema(new ApplicationMetadataSchemaKey("myprofiledetail"));
            Assert.IsTrue(schema.CommandSchema.HasDeclaration);
            var primaryBar = schema.CommandSchema.ApplicationCommands["#detail.primary"];
            var saveCommand = primaryBar.Commands.First(f => f.Id.Equals("save"));
            var comm = (ApplicationCommand) saveCommand;
            Assert.AreEqual(comm.Service, "personService");
            Assert.AreEqual(comm.Method, "submitPerson");
        }


    }
}
