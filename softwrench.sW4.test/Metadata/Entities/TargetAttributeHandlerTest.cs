using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.Internal;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata.Entities {
    [TestClass]
    public class TargetAttributeHandlerTest : BaseMetadataTest {

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "test2";
            MetadataProvider.StubReset();
        }

        [TestMethod]
        public void TestMethod1()
        {
            var metadata = MetadataProvider.Entity("SR");
            var wo = JObject.Parse(new StreamReader("jsons\\sr\\creation1.json").ReadToEnd());
            var data = EntityBuilder.BuildFromJson<CrudOperationData>(typeof(CrudOperationData), metadata, null, wo, null);

            var result = TargetAttributesHandler.SetValuesFromJSON(new ServiceIncident(), metadata,data);
            Assert.IsNotNull(result.Problem);
            Assert.AreEqual(result.WorkflowStatus, "new");
            Assert.AreEqual(result.Problem.Abstract, "test");
//            Assert.IsNotNull(result.Metrics.ProblemOccurredDateTime);
            
        }

    }
}
