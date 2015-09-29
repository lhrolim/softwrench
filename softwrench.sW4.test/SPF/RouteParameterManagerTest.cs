using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.API;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.SPF;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.SPF {
    [TestClass]
    public class RouteParameterManagerTest {

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "otb";
            MetadataProvider.StubReset();
        }

        [TestMethod]
        //test for https://controltechnologysolutions.atlassian.net/browse/SWWEB-1640
        public void TestNewSchemaRedirectingToDetail() {
            var currentApp = MetadataProvider.Application("servicerequest").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("newdetail"));
            var nextApp = RouteParameterManager.FillNextSchema(currentApp, new RouterParametersDTO(), ClientPlatform.Web, InMemoryUser.TestInstance(), null);
            Assert.AreEqual(nextApp.Schema.SchemaId, "editdetail");
        }


        [TestMethod]
        public void TestDetailSchemaStayOnIt() {
            var currentApp = MetadataProvider.Application("servicerequest").ApplyPoliciesWeb(new ApplicationMetadataSchemaKey("editdetail"));
            var nextApp = RouteParameterManager.FillNextSchema(currentApp, new RouterParametersDTO(), ClientPlatform.Web, InMemoryUser.TestInstance(), null);
            Assert.AreSame(nextApp, currentApp);
        }
    }
}
