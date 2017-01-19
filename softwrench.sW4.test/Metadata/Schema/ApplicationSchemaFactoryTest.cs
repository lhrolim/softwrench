using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.entities.security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.Schema;

namespace softwrench.sW4.test.Metadata.Schema {
    [TestClass]
    public class ApplicationSchemaFactoryTest : BaseOtbMetadataTest {

        private ApplicationSchemaDefinition _schema;

        [TestInitialize]
        public override void Init() {
            base.Init();
            _schema = MetadataProvider.Application("servicerequest").Schema(new ApplicationMetadataSchemaKey("list"));
        }

        [TestMethod]
        public void TestSecureCreation() {
            var profile = new MergedUserProfile();
            profile.Permissions = new List<ApplicationPermission>()
            {
                new ApplicationPermission()
                {
                    ApplicationName = "servicerequest",
                    AllowCreation = false
                }
            };

            Assert.IsNotNull(_schema.NewSchemaRepresentation);

            //if there´s a such allowcreation=false restriction, disable it
            var securedSchema = _schema.ApplyPolicy(new List<Role>(), ClientPlatform.Web, null, profile);
            Assert.IsNull(securedSchema.NewSchemaRepresentation);

            //if there´s no such allowcreation=false restriction, allow it
            securedSchema = _schema.ApplyPolicy(new List<Role>(), ClientPlatform.Web, null, new MergedUserProfile());
            Assert.IsNotNull(securedSchema.NewSchemaRepresentation);
        }

        [TestMethod]
        public void TestParentSchemaMerge() {
            var parentSchema = new ApplicationSchemaDefinition();
            parentSchema.Displayables.Add(CreateField("description"));
            parentSchema.Displayables.Add(CreateField("longtext"));
            parentSchema.Displayables.Add(CreateSection(null, CreateField("date")));

            var inner = CreateSection("inner", CreateField("innerField"));
            var multilevelSection = CreateSection("multi", CreateField("multiBefore"), inner, CreateField("multiAfter"));
            parentSchema.Displayables.Add(multilevelSection);
            parentSchema.Displayables.Add(CreateSection("childSection")); // empty section to get child content
            parentSchema.Displayables.Add(CreateField("signature"));

            var childSchema = new ApplicationSchemaDefinition();
            var tabSection = CreateSection(null, CreateSection("fakeTab", CreateField("tabContent")));
            var childSection = CreateSection("childSection", CreateField("childBefore"), tabSection, CreateField("childAfter"));
            childSchema.Displayables.Add(childSection);
            childSchema.Displayables.Add(CreateSection(null, CreateField("shouldNotExist1")));
            childSchema.Displayables.Add(CreateSection("shouldNotExist2", CreateField("shouldNotExist3")));
            childSchema.Displayables.Add(CreateSection("inner", CreateField("newInner")));

            var merged = ApplicationSchemaFactory.MergeParentSchemaDisplayables(childSchema, parentSchema);


            Assert.AreEqual(6, merged.Count);

            Assert.AreEqual("description", ((ApplicationFieldDefinition)merged[0]).Attribute);

            Assert.AreEqual("longtext", ((ApplicationFieldDefinition)merged[1]).Attribute);

            var nullSectionCheck = ((ApplicationSection)merged[2]);
            Assert.IsNull(nullSectionCheck.Id);
            Assert.AreEqual(1, nullSectionCheck.Displayables.Count);
            Assert.AreEqual("date", ((ApplicationFieldDefinition)nullSectionCheck.Displayables[0]).Attribute);

            var multiSectionCheck = ((ApplicationSection)merged[3]);
            Assert.AreEqual("multi", multiSectionCheck.Id);
            Assert.AreEqual(3, multiSectionCheck.Displayables.Count);
            Assert.AreEqual("multiBefore", ((ApplicationFieldDefinition)multiSectionCheck.Displayables[0]).Attribute);
            var innerSectionCheck = (ApplicationSection)multiSectionCheck.Displayables[1];
            Assert.AreEqual("inner", innerSectionCheck.Id);
            Assert.AreEqual(1, innerSectionCheck.Displayables.Count);
            Assert.AreEqual("newInner", ((ApplicationFieldDefinition)innerSectionCheck.Displayables[0]).Attribute);
            Assert.AreEqual("multiAfter", ((ApplicationFieldDefinition)multiSectionCheck.Displayables[2]).Attribute);

            var childSectionCheck = ((ApplicationSection)merged[4]);
            Assert.AreEqual("childSection", childSectionCheck.Id);
            Assert.AreEqual(3, childSectionCheck.Displayables.Count);
            Assert.AreEqual("childBefore", ((ApplicationFieldDefinition)childSectionCheck.Displayables[0]).Attribute);
            var tabSectionCheck = (ApplicationSection)childSectionCheck.Displayables[1];
            Assert.IsNull(tabSectionCheck.Id);
            Assert.AreEqual(1, tabSectionCheck.Displayables.Count);
            var fakeTabSection = (ApplicationSection)tabSectionCheck.Displayables[0];
            Assert.AreEqual("fakeTab", fakeTabSection.Id);
            Assert.AreEqual(1, fakeTabSection.Displayables.Count);
            Assert.AreEqual("tabContent", ((ApplicationFieldDefinition)fakeTabSection.Displayables[0]).Attribute);
            Assert.AreEqual("childAfter", ((ApplicationFieldDefinition)childSectionCheck.Displayables[2]).Attribute);

            Assert.AreEqual("signature", ((ApplicationFieldDefinition)merged[5]).Attribute);
        }

        private static ApplicationFieldDefinition CreateField(string att) {
            return new ApplicationFieldDefinition { Attribute = att };
        }

        private static ApplicationSection CreateSection(string id, params IApplicationAttributeDisplayable[] displayables) {
            var section = new ApplicationSection { Id = id };
            if (displayables == null) return section;
            section.Displayables.AddRange(displayables);
            return section;
        }
    }
}
