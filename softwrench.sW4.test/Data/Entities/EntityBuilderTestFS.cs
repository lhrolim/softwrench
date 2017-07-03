using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using softwrench.sW4.TestBase;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sW4.test.Data.Entities {
    [TestClass]
    public class EntityBuilderTestFS : BaseMetadataTest {


        [TestInitialize]
        public void Init() {
            if (ApplicationConfiguration.TestclientName != "firstsolar") {
                ApplicationConfiguration.TestclientName = "firstsolar";
                MetadataProvider.StubReset();
            }
        }


        [TestMethod]
        public void TestCreation() {
            MetadataProvider.StubReset();
            var metadata = MetadataProvider.Entity("workpackage_");

            var wp = JObject.Parse(new StreamReader("jsons\\workpackage\\schemaloading.json").ReadToEnd());

            var applicationMetadata = MetadataProvider
                .Application("_workpackage")
                .ApplyPolicies(new Shared2.Metadata.Applications.Schema.ApplicationMetadataSchemaKey("adetail"), InMemoryUser.TestInstance("swadmin"), ClientPlatform.Web);

            var entity = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), metadata, applicationMetadata, wp);

            Assert.IsNotNull(entity.GetAttribute("#workorder_.worktype"));

            var rels = entity.GetRelationship("dailyOutageMeetings_") as List<CrudOperationData>;

            Assert.IsNotNull(rels);

            Assert.AreEqual(new Decimal(1.1),rels[0].GetAttribute("mwhlostyesterday"));



        }






    }
}
