using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Validator;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata.Parsing {
    [TestClass]
    public class XmlMCommandBarMetadataParserTest {


        [TestMethod]
        public void TestMethod1() {
            ApplicationConfiguration.TestclientName = "test_only";
            var commands =new XmlCommandBarMetadataParser().Parse(MetadataParsingUtils.GetStreamImpl("commands_test.xml"));
            Assert.IsTrue(commands.ContainsKey("#a.d"));
            Assert.IsTrue(commands.ContainsKey("#a.b"));
            Assert.IsTrue(commands.ContainsKey("#a.b.c"));

            var abCommandBar = commands["#a.b"];
            Assert.AreEqual(2, abCommandBar.Commands.Count);

            Assert.AreEqual("c", abCommandBar.Commands[0].Id);
            Assert.AreEqual("c1", abCommandBar.Commands[1].Id);


            var abcCommand =commands["#a.b.c"];
            Assert.AreEqual(3,abcCommand.Commands.Count);

            Assert.AreEqual("c",abcCommand.Commands[0].Id);
            Assert.AreEqual("c1",abcCommand.Commands[1].Id);
            Assert.AreEqual("c2",abcCommand.Commands[2].Id);

        }
    }
}
