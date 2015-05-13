using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Persistence.WS.Commons;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.Imac;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;
using System.Diagnostics;
using System.IO;

namespace softwrench.sW4.test.Data.Persistence.WS.Ism {



    [TestClass]
    public class OfferingDescriptionHandlerTest {


        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "hapag";
            MetadataProvider.StubReset();
        }

        [TestMethod]
        public void Windowsserver() {
            DoTest("serverwindows", "serverwindows.json", "serverwindows.txt");
        }


        private static void DoTest(string schemaId, string input, string output) {
            var appMetadata = MetadataProvider.Application("offering")
                .ApplyPolicies(new ApplicationMetadataSchemaKey(schemaId), InMemoryUser.TestInstance("test"),
                    ClientPlatform.Web);
            var metadata = MetadataProvider.Entity("offering");
            var imacjson = JObject.Parse(new StreamReader("jsons\\offering\\" + input).ReadToEnd());
            var offeringData = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), metadata, null, imacjson, null);
            offeringData.ApplicationMetadata = appMetadata;
            var resultDescription = HapagOfferingLongDescriptionHandler.ParseSchemaBasedLongDescription(offeringData);
            Debug.Write(resultDescription);
            var expectedResult = new StreamReader("jsons\\offering\\descriptionresults\\" + output).ReadToEnd();
            Assert.AreEqual(expectedResult, resultDescription);
        }




    }
}
