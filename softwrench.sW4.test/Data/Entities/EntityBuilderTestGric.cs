using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Entities {
    [TestClass]
    public class EntityBuilderTestGric {

        [TestMethod]
        [Ignore]
        public void TestAddWorkLogToIncident() {
            ApplicationConfiguration.TestclientName = "gric";
            MetadataProvider.StubReset();

            var metadata = MetadataProvider.Entity("SR");
            var completeApp = MetadataProvider.Application("servicerequest");
            var sliced =MetadataProvider.SlicedEntityMetadata(completeApp.ApplyPolicies(
                new ApplicationMetadataSchemaKey("editdetail"), InMemoryUser.TestInstance("test"), ClientPlatform.Web));
            var attributes = sliced.NonListAssociations();
            Assert.IsTrue(attributes.Count(a => a.Qualifier=="asset_")==1);

            

        }
    }
}
