using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
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
    public class ImacDescriptionHandlerTest {


        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "hapag";
            MetadataProvider.StubReset();
        }

        [TestMethod]
        public void Move() {
            DoTest("move", "move_other_location.json", "move_other_location.txt");
        }


        [TestMethod]
        [Ignore]
        public void InstallLan() {
            DoTest("installlan", "installlan.json", "installlan.txt");
        }

        [TestMethod]
        public void ReplaceAsset() {
            DoTest("replaceother", "replaceother.json", "replaceother.txt");
        }

//        [TestMethod]
//        [Ignore]
//        public void UpdateAsset() {
//            DoTest("update", "update_asset_data.json", "update_asset_data.txt");
//        }

        [TestMethod]
        public void RemoveAsset() {
            DoTest("removeother", "removeother.json", "removeother.txt");
        }

        private static void DoTest(string schemaId, string input, string output) {
            var appMetadata = MetadataProvider.Application("imac")
                .ApplyPolicies(new ApplicationMetadataSchemaKey(schemaId), InMemoryUser.TestInstance("test"),
                    ClientPlatform.Web);
            var metadata = MetadataProvider.Entity("imac");
            var imacjson = JObject.Parse(new StreamReader("jsons\\imac\\" + input).ReadToEnd());
            var imac = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), metadata, null, imacjson, null);
            var resultDescription = ImacDescriptionHandler.BuildDescription(imac, appMetadata);
            Debug.Write(resultDescription);
            var result = new StreamReader("jsons\\imac\\descriptionresults\\" + output).ReadToEnd();
            Assert.AreEqual(result, resultDescription);
        }




    }
}
