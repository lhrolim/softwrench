using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.dynforms.classes.model.entity;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softwrench.sW4.Shared2.Metadata.Entity.Association;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Definitions.WhereClause;
using softWrench.sW4.Data.Entities.Attachment;
using softWrench.sW4.Dynamic.Model;
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
            var propertyValueRel = rels.FirstOrDefault(f => f.To.EqualsIc("_propertyvalue_"));
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
            var attributes = entityMetadata.Schema.Attributes;
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

        [TestMethod]
        public void TestEnums() {
            var service = new SWDBMetadataXmlSourceInitializer();
            var entityMetadata = service.Convert(typeof(JavascriptEntry));
            var attributes = entityMetadata.Attributes(EntityMetadata.AttributesMode.NoCollections);
            Assert.IsTrue(attributes.Any(a=> a.Name.EqualsIc("offlinedevice")));
            Assert.IsTrue(attributes.Any(a=> a.Name.EqualsIc("platform")));

        }

        [TestMethod]
        public void TestAttributes() {
            var service = new SWDBMetadataXmlSourceInitializer();
            var entityMetadata = service.Convert(typeof(WorkPackage));
            var attributes = entityMetadata.Attributes(EntityMetadata.AttributesMode.NoCollections);
            Assert.IsTrue(attributes.Any(a => a.Name.EqualsIc("subContractorEnabled")));
            var dateAttr = attributes.FirstOrDefault(f => f.Name.EqualsIc("createddate"));
            Assert.IsNotNull(dateAttr);
            Assert.AreEqual("datetime",dateAttr.Type);

            var id = attributes.FirstOrDefault(f => f.Name.EqualsIc("id"));
            Assert.IsNotNull(id);
            Assert.AreEqual("int32", id.Type);
        }


        [TestMethod]
        public void TestOneToManyRelationship() {
            var service = new SWDBMetadataXmlSourceInitializer();
            var entityMetadata = service.Convert(typeof(WorkPackage));
            var associations = entityMetadata.Associations;
            Assert.IsNotNull(associations);
            var genRelationship = associations.FirstOrDefault(f => f.Qualifier.EqualsIc("genericrelationships_"));
            Assert.IsNotNull(genRelationship);

            Assert.AreEqual("_genericlistrelationship_", genRelationship.To);
//            Assert.AreEqual("_genericlistrelationship", genRelationship.EntityName);
            Assert.IsTrue(genRelationship.Collection);
            Assert.IsTrue(genRelationship.IsSwDbApplication);
            var entityAssociationAttributes2 = genRelationship.Attributes;

            var attributes = entityAssociationAttributes2 as IList<EntityAssociationAttribute> ?? entityAssociationAttributes2.ToList();

            Assert.AreEqual(2, attributes.Count());
            var first = attributes[0];
            Assert.AreEqual("id", first.From);
            Assert.AreEqual("parentid", first.To);
            Assert.IsTrue(first.Primary);

            var second = attributes[1];
            Assert.AreEqual("ParentEntity = 'WorkPackage'", second.Query);
            Assert.IsFalse(second.Primary);


        }

        [TestMethod]
        public void TestManyToOneRelationship() {
            var service = new SWDBMetadataXmlSourceInitializer();
            var entityMetadata = service.Convert(typeof(DocLink));
            //cannot test api call without initializing metadata provider
            var entityAttributes = entityMetadata.Schema.Attributes;
            Assert.IsNotNull(entityAttributes.FirstOrDefault(f=> f.Name.Equals("docinfo_id")));
            var associations = entityMetadata.Associations;
            Assert.IsNotNull(associations);
            var docInfo = associations.FirstOrDefault(f => f.Qualifier.EqualsIc("docinfo_"));
            Assert.IsNotNull(docInfo);

            Assert.AreEqual("_docinfo_", docInfo.To);
            //            Assert.AreEqual("_genericlistrelationship", genRelationship.EntityName);
            Assert.IsFalse(docInfo.Collection);
            Assert.IsTrue(docInfo.IsSwDbApplication);
            var entityAssociationAttributes2 = docInfo.Attributes;

            var attributes = entityAssociationAttributes2 as IList<EntityAssociationAttribute> ?? entityAssociationAttributes2.ToList();

            Assert.AreEqual(1, attributes.Count());
            var first = attributes[0];
            Assert.AreEqual("docinfo_id", first.From);
            Assert.AreEqual("id", first.To);
            Assert.IsTrue(first.Primary);
        }

        [TestMethod]
        public void TestManyToOneRelationshipWithIds() {
            var service = new SWDBMetadataXmlSourceInitializer();
            var entityMetadata = service.Convert(typeof(FormDatamap));
            //cannot test api call without initializing metadata provider
            var entityAttributes = entityMetadata.Schema.Attributes;
            var associations = entityMetadata.Associations;
            Assert.IsNotNull(associations);
            var formMetadataRel = associations.FirstOrDefault(f => f.Qualifier.EqualsIc("formmetadata_"));
            Assert.IsNotNull(formMetadataRel);

            Assert.AreEqual("_formmetadata_", formMetadataRel.To);
            //            Assert.AreEqual("_genericlistrelationship", genRelationship.EntityName);
            Assert.IsFalse(formMetadataRel.Collection);
            Assert.IsTrue(formMetadataRel.IsSwDbApplication);
            var entityAssociationAttributes2 = formMetadataRel.Attributes;

            var attributes = entityAssociationAttributes2 as IList<EntityAssociationAttribute> ?? entityAssociationAttributes2.ToList();

            Assert.AreEqual(1, attributes.Count());
            var first = attributes[0];
            Assert.AreEqual("form_name", first.From);
            Assert.AreEqual("name", first.To);
            Assert.IsTrue(first.Primary);
        }

        [TestMethod]
        public void TestId() {
            var service = new SWDBMetadataXmlSourceInitializer();
            var entityMetadata = service.Convert(typeof(FormMetadata));
            var attributes = entityMetadata.Attributes(EntityMetadata.AttributesMode.NoCollections);
            Assert.IsTrue(attributes.Any(a => a.Name.EqualsIc("name")));

            entityMetadata = service.Convert(typeof(FormDatamap));

            Assert.AreEqual(entityMetadata.IdFieldName, "formdatamapid");

        }
    }
}
