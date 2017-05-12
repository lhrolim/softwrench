using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata.Entities {

    [TestClass]
    public class FsEntityMetadataTest : BaseOtbMetadataTest {


        [TestMethod]
        public void TestAssociationLookupWithTransientValue() {
            if (ApplicationConfiguration.TestclientName != GetClientName()) {
                ApplicationConfiguration.TestclientName = GetClientName();
                MetadataProvider.StubReset();
            }
            var metadata = MetadataProvider.Entity("workorder");
            var woResult = metadata.LocateAssociationByLabelField("worktype_.wtypedesc");
            var originalAssociation = woResult.Item1;

            metadata = MetadataProvider.Entity("workpackage_");
            var result = metadata.LocateAssociationByLabelField("#workorder_.worktype_.wtypedesc");

            var transientAssociation = result.Item1;

            Assert.IsFalse(originalAssociation.IsTransient);
            Assert.IsTrue(transientAssociation.IsTransient);

            Assert.AreEqual(woResult.Item2, result.Item2);
            Assert.AreEqual(originalAssociation.EntityName, transientAssociation.EntityName);
            

            var originalAttributes = originalAssociation.Attributes.ToList();
            var transientAttributes = transientAssociation.Attributes.ToList();

            Assert.AreEqual("worktype", originalAttributes[0].To);
            Assert.AreEqual("orgid", originalAttributes[1].To);

            Assert.AreEqual(originalAttributes[0].To, transientAttributes[0].To);
            Assert.AreEqual(originalAttributes[0].Primary, transientAttributes[0].Primary);
            Assert.AreEqual(originalAttributes[1].To, transientAttributes[1].To);

            Assert.AreEqual("worktype", originalAttributes[0].From);

            Assert.AreEqual("#workorder_." + originalAttributes[0].From, transientAttributes[0].From);
            Assert.AreEqual("#workorder_." + originalAttributes[1].From, transientAttributes[1].From);

        }

        [TestMethod]
        public void TestStatusAssociation() {
            if (ApplicationConfiguration.TestclientName != GetClientName()) {
                ApplicationConfiguration.TestclientName = GetClientName();
                MetadataProvider.StubReset();
            }

            var metadata = MetadataProvider.Entity("workpackage_");
            var result = metadata.LocateAssociationByLabelField("#workorder_.synstatus.description");

            var transientAssociation = result.Item1;

            Assert.IsTrue(transientAssociation.IsTransient);

            var listAttributes = transientAssociation.Attributes.ToList();

            Assert.AreEqual(2, listAttributes.Count);

            Assert.AreEqual("domainid", listAttributes[1].To);
            Assert.AreEqual("WOSTATUS", listAttributes[1].Literal);
            Assert.IsTrue(listAttributes[1].QuoteLiteral);
            Assert.IsNull(listAttributes[1].From);



        }

        public override string GetClientName() {
            return "firstsolar";
        }

    }
}
