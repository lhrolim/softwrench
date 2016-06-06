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
using softwrench.sW4.TestBase;

namespace softwrench.sW4.test.Data.Entities {
    [TestClass]
    public class EntityBuilderTestHapag : BaseMetadataTest {

        [TestMethod]
        public void TestAddWorkLogToIncident() {
            ApplicationConfiguration.TestclientName = "hapag";
            MetadataProvider.StubReset();
            
            var imac = MetadataProvider.Entity("imac");

            var metadata = MetadataProvider.Entity("incident");
            var completeApp = MetadataProvider.Application("incident");

            var sliced =MetadataProvider.SlicedEntityMetadata(completeApp.ApplyPolicies(new ApplicationMetadataSchemaKey("detail"), InMemoryUser.TestInstance("test"), ClientPlatform.Web));
            var attributes = sliced.Attributes(EntityMetadata.AttributesMode.NoCollections);
            var entityAttributes = attributes as EntityAttribute[] ?? attributes.ToArray();
            Assert.IsNull(entityAttributes.FirstOrDefault(a => a.Name == "asset_.eq23"));
            Assert.IsNotNull(entityAttributes.FirstOrDefault(a => a.Name == "asset_.description"));
            var incident = JObject.Parse(new StreamReader("jsons\\incident\\test1.json").ReadToEnd());
            var entity = EntityBuilder.BuildFromJson<Entity>(typeof(Entity), metadata, null, incident, null);

            Assert.IsNull(entity.GetAttribute("wonum"));

            var solution = entity.GetRelationship("solution") as Entity;
            Assert.IsNotNull(solution);
            var cause =solution.GetRelationship("cause") as Entity;
            Assert.IsNotNull(cause);
            Assert.IsNull(cause.GetAttribute("ldtext"));

            var symptom = solution.GetRelationship("symptom") as Entity;
            Assert.IsNotNull(symptom);
            Assert.IsNotNull(symptom.GetAttribute("ldtext"));

        }


        [TestMethod]
        public void TestNestedGetAttribute() {
            ApplicationConfiguration.TestclientName = "hapag";
            MetadataProvider.StubReset();

            var appMetadata= MetadataProvider.Application("imac").ApplyPolicies(new ApplicationMetadataSchemaKey("install"), InMemoryUser.TestInstance("test"),
                ClientPlatform.Web);
            var metadata = MetadataProvider.Entity("imac");
            var incident = JObject.Parse(new StreamReader("jsons\\sr\\updateasset.json").ReadToEnd());
            var entity = EntityBuilder.BuildFromJson<Entity>(typeof(Entity), metadata, appMetadata, incident, null);
            var assetRel = entity.GetRelationship("asset_") as Entity;
            Assert.IsNotNull(assetRel);
            var primaryUser =assetRel.GetRelationship("primaryuser_") as Entity;
            Assert.IsNotNull(primaryUser);
            var personId =primaryUser.GetAttribute("personid");
            Assert.AreEqual(entity.GetAttribute("asset_.primaryuser_.personid"), personId);


        }
    }
}
