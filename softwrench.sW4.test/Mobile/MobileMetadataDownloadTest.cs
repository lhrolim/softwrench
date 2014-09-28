using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Offline;
using softWrench.sW4.Security.Services;
using softwrench.sW4.Shared.Metadata.Applications;
using softwrench.sW4.Shared.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Mobile {
    [TestClass]
    public class MobileMetadataDownloadTest {
        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "entegra";
            MetadataProvider.StubReset();
        }

        [TestMethod]
        public void JsonEntegra() {
            var user = SecurityFacade.CurrentUser();
            var metadatas = MetadataProvider.Applications(ClientPlatform.Mobile);
            var securedMetadatas = metadatas.Select(metadata => MobileCompleteApplicationMetadata.CloneSecuring(metadata, user)).ToList();
            var workorder = securedMetadatas.First(f => f.Name == "workorder");
            Assert.AreEqual(2, workorder.Schemas.Count);
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;
            var json = JsonConvert.SerializeObject(securedMetadatas, Formatting.Indented);
            Debug.Write(json);
            //            json = json.Replace("softWrench.sW4.Metadata.Applications.Schema.ApplicationSchema, softWrench.sW4", "" + typeof(TestAppSchema).AssemblyQualifiedName);
            //            Debug.Write(json);
            //            var deserializedMetadatas = JsonConvert.DeserializeObject<IEnumerable<MobileCompleteApplicationMetadata>>(json);
            //            Assert.IsTrue(deserializedMetadatas.First().Schemas.First().Value is TestAppSchema);
        }

     

//        public class TestAppSchema : ApplicationSchemaDefinition { }
    }
}
