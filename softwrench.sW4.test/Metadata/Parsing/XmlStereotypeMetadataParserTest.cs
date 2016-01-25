using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata.Parsing;
using softWrench.sW4.Metadata.Validator;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata.Parsing {
    [TestClass]
    public class XmlStereotypeMetadataParserTest {


        [TestMethod]
        public void TestNestedStereotypes() {
            ApplicationConfiguration.TestclientName = "test_only";
            var stereotypes =new XmlStereotypeMetadataParser().Parse(MetadataParsingUtils.GetStreamImpl("stereotypes_test.xml"),false);
            Assert.IsTrue(stereotypes.ContainsKey("list.selection"));
            Assert.IsTrue(stereotypes.ContainsKey("list"));
            var stereotype =stereotypes["list.selection"];
            var listStereotype =stereotypes["list"];
            Assert.AreEqual(listStereotype.Properties.Count + 2,stereotype.Properties.Count);

        }
    }
}
