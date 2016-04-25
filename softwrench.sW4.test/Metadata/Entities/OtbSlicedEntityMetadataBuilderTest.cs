using System.Collections.Generic;
using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Util;
using System.Diagnostics;
using System.Linq;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;

namespace softwrench.sW4.test.Metadata.Entities {

    [TestClass]
    public class OtbSlicedEntityMetadataBuilderTest : BaseMetadataTest {

        private static ApplicationSchemaDefinition _incidentschema;
        private static ApplicationSchemaDefinition _srschema;
        private static ApplicationSchemaDefinition _labTransSchema;

        [TestInitialize]
        public void Init() {
            ApplicationConfiguration.TestclientName = "otb";
            MetadataProvider.StubReset();
            var schemas = MetadataProvider.Application("incident").Schemas();
            _incidentschema = schemas[new ApplicationMetadataSchemaKey("editdetail", "input", "web")];

            schemas = MetadataProvider.Application("servicerequest").Schemas();
            _srschema = schemas[new ApplicationMetadataSchemaKey("editdetail", "input", "web")];

            _labTransSchema = MetadataProvider.Application("labtrans").Schemas()[new ApplicationMetadataSchemaKey("editdetail", "input", "web")];
        }

        [TestMethod]
        public void TestSelect() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("incident"), _incidentschema);
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
        public void TestSelectMultiLevel() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("labtrans"), _labTransSchema);
            var attributes = sliced.Attributes(EntityMetadata.AttributesMode.NoCollections);
            var entityAttributes = attributes as IList<EntityAttribute> ?? attributes.ToList();
            Assert.IsTrue(entityAttributes.Any(a => a.Name.CountNumberOfOccurrences('_') > 1));
            Assert.IsFalse(entityAttributes.Any(a => a.Name.CountNumberOfOccurrences('.') > 1));

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
