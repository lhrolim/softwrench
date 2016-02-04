using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Util;
using System.Diagnostics;

namespace softwrench.sW4.test.Metadata.Entities {
    
    [TestClass]
    public class OtbSlicedEntityMetadataBuilderTest {
        
        private static ApplicationSchemaDefinition _incidentschema;
        private static ApplicationSchemaDefinition _srschema;

        [TestInitialize]
        public void Init() {
            ApplicationConfiguration.TestclientName = "otb";
            MetadataProvider.StubReset();
            var schemas = MetadataProvider.Application("incident").Schemas();
            _incidentschema = schemas[new ApplicationMetadataSchemaKey("editdetail", "input", "web")];

            schemas = MetadataProvider.Application("servicerequest").Schemas();
            _srschema = schemas[new ApplicationMetadataSchemaKey("editdetail", "input", "web")];
        }

        [TestMethod]
        public void TestSelect() {
            var sliced =SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("incident"), _incidentschema);
            var select = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.Detail);
            Debug.Write(select);
            Assert.IsFalse(select.Contains("person_.langcode"));
            Assert.IsFalse(select.Contains("person_.displayname"));
            //non associations should get fetched, unless marked as lazy
            Assert.IsTrue(select.Contains("resolution_.ldtext"));
        }

        [TestMethod]
        public void TestSelect2() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("sr"), _srschema);
            var select = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.Detail);
            Debug.Write(select);
            //non associations should get fetched, unless marked as lazy
            Assert.IsTrue(select.Contains("ownerperson_.displayname"));
        }


        [TestMethod]
        public void TestFrom() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("incident"), _incidentschema);
            var from = QueryFromBuilder.Build(sliced);
            Debug.Write(from);
            //assert that the relationships are not being duplicated
            Assert.AreEqual(0, from.GetNumberOfItems("solution_.solution"));
        }
        
    }
}
