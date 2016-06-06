using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.TestBase;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Metadata;
using softWrench.sW4.mif_sr;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata.Entities {
    [TestClass]
    public class TargetConstantHandlerTest : BaseMetadataTest {

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitialize]
        public void Init() {
            ApplicationConfiguration.TestclientName = "test2";
            MetadataProvider.StubReset();
        }

        [TestMethod]
        public void TestMethod1() {
            var result = TargetConstantHandler.SetConstantValues(new ServiceIncident(), MetadataProvider.Entity("SR"));
            Assert.IsNotNull(result.Problem);
            Assert.AreEqual(result.Problem.System, "21030000");
            Assert.IsNotNull(result.Problem.ProviderAssignedGroup);
            Assert.IsNotNull(result.Problem.ProviderAssignedGroup.Group);
            Assert.AreEqual(result.Problem.ProviderAssignedGroup.Group.GroupID, "I-EUS-DE-CSC-SDK-HLCFRONTDESKI");
            Assert.IsNotNull(result.Problem.ProviderAssignedGroup.Group.Address);
            Assert.AreEqual(result.Problem.ProviderAssignedGroup.Group.Address.OrganizationID, "ITD-ESS6");
            Assert.AreEqual(result.Problem.ProviderAssignedGroup.Group.Address.LocationID, "ESS6");
            Assert.IsNotNull(result.Transaction);

        }

        [TestMethod]
        public void TestAlreadyFilledValues() {
            var sr = new ServiceIncident();
            sr.Problem = new Problem();
            sr.Problem.Abstract = "true";
            var result = TargetConstantHandler.SetConstantValues(sr, MetadataProvider.Entity("SR"));
            Assert.IsNotNull(result.Problem);
            Assert.AreEqual(result.Problem.System, "21030000");
            Assert.AreEqual(result.Problem.Abstract, "true");
            
        }

        [TestMethod]
        public void TestConversion() {
            ApplicationConfiguration.TestclientName = "test3";
            MetadataProvider.StubReset();
            var mifsr =new MXSW4SR_SRType();
            var result = TargetConstantHandler.SetConstantValues(mifsr, MetadataProvider.Entity("SR"));
            Assert.IsNotNull(mifsr.ACTLABHRS);
            Assert.AreEqual(mifsr.ACTLABHRS.Value,0);
            

        }
    }
}
