using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Metadata.Entities;
using softWrench.sW4.Metadata.Entities.Schema;
using softWrench.sW4.Metadata.Validator;

namespace softwrench.sW4.test.Metadata.Validator {
    [TestClass]
    public class SWDBMetadataXmlSourceInitializerTest {

        [TestMethod]
        public void TestGenerationWithEmbeddables() {
            var service = new SWDBMetadataXmlSourceInitializer();
            var entityMetadata = service.Convert(typeof(WhereClauseCondition));
            var attributes = entityMetadata.Attributes(EntityMetadata.AttributesMode.NoCollections);
            var entityAttributes = attributes as IList<EntityAttribute> ?? attributes.ToList();
            Assert.IsTrue(entityAttributes.Any(f => f.Name.EqualsIc("MetadataId")));
            Assert.IsTrue(entityAttributes.Any(f => f.Name.EqualsIc("offlineonly")));
            Assert.AreEqual(typeof(WhereClauseCondition),entityMetadata.BackEndType);
        }


        [TestMethod]
        public void TestGenerationWithOneToManyRel() {
            var service = new SWDBMetadataXmlSourceInitializer();
            var entityMetadata = service.Convert(typeof(PropertyDefinition));
            Assert.IsNull(entityMetadata.Attributes(EntityMetadata.AttributesMode.NoCollections).FirstOrDefault(f => f.Name == "rowstamp"));
            var rels = entityMetadata.Associations;
            var propertyValueRel = rels.FirstOrDefault(f => f.To.EqualsIc("propertyvalue_"));
            Assert.IsNotNull(propertyValueRel);
            Assert.IsTrue(propertyValueRel.Collection);
            Assert.AreEqual("values_", propertyValueRel.Qualifier);
            var primary = propertyValueRel.PrimaryAttribute();
            Assert.AreEqual("definition_id", primary.To);
            Assert.AreEqual("fullkey", primary.From);

        }

        [TestMethod]
        public void TestGenerationWithDifferentColumnName() {
            var service = new SWDBMetadataXmlSourceInitializer();
            var entityMetadata = service.Convert(typeof(PropertyValue));
            var attributes = entityMetadata.Attributes(EntityMetadata.AttributesMode.NoCollections);
            Assert.IsNull(attributes.FirstOrDefault(f => f.Name == "rowstamp"));
            Assert.IsNull(attributes.FirstOrDefault(f => f.Name.EqualsIc("definition")));
            var defId = attributes.FirstOrDefault(f => f.Name.EqualsIc("definition_id"));
            Assert.IsNotNull(defId);
            Assert.AreEqual(defId.Type, "varchar");


        }

        [TestMethod]
        public void TestGettingRightIdName() {
            var service = new SWDBMetadataXmlSourceInitializer();
            var entityMetadata = service.Convert(typeof(WhereClauseCondition));
            Assert.AreEqual("wcwcid", entityMetadata.IdFieldName);

        }
    }
}
