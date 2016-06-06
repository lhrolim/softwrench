using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;
using softwrench.sW4.TestBase;

namespace softwrench.sW4.test.Data.Persistence.Relational.QueryBuilder.Basic {



    [TestClass]
    public class QueryFromBuilderTest : BaseMetadataTest {

        [TestInitialize]
        public void Init() {
            if (ApplicationConfiguration.TestclientName != "hapag") {
                ApplicationConfiguration.TestclientName = "hapag";
                MetadataProvider.StubReset();
            }
        }

        [TestMethod]
        public void TestMethod1() {
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(new ProjectionField("primaryuser_.personid,", "primaryuser_.personid"));
            dto.AppendProjectionField(new ProjectionField("primaryuser_person_.displayname", "primaryuser_person_.displayname"));
            var result = QueryFromBuilder.Build(MetadataProvider.Entity("asset"), dto);
            Assert.IsTrue(result.Contains("left join person as primaryuser_person_ on (primaryuser_.personid = primaryuser_person_.personid)"));

        }

        [TestMethod]
        public void IncidentHardwareReport() {
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(ProjectionField.Default("asset_assetloccomm_commoditiesownedby_.description"));
            dto.AppendProjectionField(ProjectionField.Default("asset_assetloccomm_.commodity"));

            var completeOne = MetadataProvider.Application("incident");
            var metadata = completeOne.ApplyPolicies(new ApplicationMetadataSchemaKey("hardwarerepair"), InMemoryUser.TestInstance(),
                ClientPlatform.Web);
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("incident"), metadata.Schema);
            var result = QueryFromBuilder.Build(sliced, dto);
            Assert.AreEqual("from incident as incident left join asset as asset_ on (incident.assetnum = asset_.assetnum and incident.siteid = asset_.siteid)left join assetloccomm as asset_assetloccomm_ on (asset_.assetnum = asset_assetloccomm_.assetnum and asset_.siteid = asset_assetloccomm_.siteid)left join commodities as asset_assetloccomm_commoditiesownedby_ on (asset_assetloccomm_.commodity = asset_assetloccomm_commoditiesownedby_.commodity and asset_assetloccomm_.itemsetid = asset_assetloccomm_commoditiesownedby_.itemsetid and (asset_assetloccomm_commoditiesownedby_.description like 'Asset owened by%'))", result);
        }



        [TestMethod]
        public void TestRelationshipQuery() {
            var dto = new SearchRequestDto();
            dto.AppendProjectionField(ProjectionField.Default("description"));
            dto.AppendSearchEntry("status", "('120 Active','OPERATING')");
            dto.AppendSearchEntry("usercustodianuser_.personid", "xxx");
            var result = QueryFromBuilder.Build(MetadataProvider.Entity("asset"), dto);
            Assert.AreEqual("from asset as asset left join AssetUserCust as usercustodianuser_ on (asset.assetnum = usercustodianuser_.assetnum and asset.siteid = usercustodianuser_.siteid and usercustodianuser_.isuser = 1)", result);

        }


        //Literals standing on the from side
        [TestMethod]
        public void TestInverseRelationship() {
            //            var dto = new SearchRequestDto();
            //            var completeOne = MetadataProvider.Application("change");
            //            var metadata = completeOne.ApplyPolicies(new ApplicationMetadataSchemaKey("list"), InMemoryUser.TestInstance(),
            //                ClientPlatform.Web);
            //            ApplicationSchemaDefinition schema;
            //            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("change"), metadata.Schema);
            //            var result = QueryFromBuilder.Build(MetadataProvider.Entity("wochange"), dto);

            var dto = new SearchRequestDto();
            dto.AppendProjectionField(ProjectionField.Default("sr_.description"));
            var result = QueryFromBuilder.Build(MetadataProvider.Entity("wochange"), dto);
            Assert.AreEqual("from wochange as wochange left join SR as sr_ on (wochange.origrecordid = sr_.ticketid and wochange.origrecordclass = 'SR' and wochange.woclass = 'CHANGE')", result);
        }

        //        [TestMethod]
        //        public void TestRelationshipQueryNegative() {
        //            var dto = new SearchRequestDto();
        //            dto.AppendSearchEntry("assetusercustlistreport_.personid", "xxx");
        //            var result = QueryFromBuilder.Build(MetadataProvider.Entity("asset"), dto);
        //            Assert.AreEqual("from asset as asset left join AssetUserCust as assetusercustlistreport_ on (asset.assetnum = assetusercustlistreport_.assetnum and asset.siteid = assetusercustlistreport_.siteid and assetusercustlistreport_.itdassetrole != 'Owner')", result);
        //
        //        }

        //        [TestMethod]
        //        public void TestRelationshipQueryStartsWith() {
        //            var dto = new SearchRequestDto();
        //            dto.AppendSearchEntry("assetloccommid_.commodity", "xxx");
        //            var result = QueryFromBuilder.Build(MetadataProvider.Entity("asset"), dto);
        //            Assert.AreEqual("from asset as asset left join assetloccomm as assetloccommid_ on (asset.assetnum = assetloccommid_.assetnum and asset.siteid = assetloccommid_.siteid and assetloccommid_.commodity like 'Asset owened by%')", result);
        //
        //        }

        [TestMethod]
        public void TestRelationshipQueryField() {
            var dto = new SearchRequestDto();
            dto.AppendSearchEntry("aucisnotowner_.personid", "xxx");
            var result = QueryFromBuilder.Build(MetadataProvider.Entity("asset"), dto);
            Assert.AreEqual("from asset as asset left join AssetUserCust as aucisnotowner_ on (asset.assetnum = aucisnotowner_.assetnum and asset.siteid = aucisnotowner_.siteid and (aucisnotowner_.itdassetrole is null or aucisnotowner_.itdassetrole !='Owner') and aucisnotowner_.isuser = 1)", result);

        }

    }
}
