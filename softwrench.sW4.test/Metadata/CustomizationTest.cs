﻿using System.Collections.Generic;
using System.Linq;
using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.Shared2.Metadata.Applications.UI;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Util;
using softwrench.sW4.TestBase;

namespace softwrench.sW4.test.Metadata {
    [TestClass]
    public class CustomizationTest : BaseMetadataTest {

        private IList<ApplicationFieldDefinition> _baseWorklogDisplayables;

        private List<IApplicationDisplayable> _baseIssueDisplayables;

        [TestInitialize]
        public void Init() {
            if (ApplicationConfiguration.TestclientName != "otb") {
                ApplicationConfiguration.TestclientName = "otb";
                MetadataProvider.StubReset();
            }


            /* Temporarily used in TestAfterAndBefore1() Method */
            var app = MetadataProvider.Application("worklog");
            var listSchema = app.Schema(new ApplicationMetadataSchemaKey("list"));

            _baseWorklogDisplayables = listSchema.Fields;

            app = MetadataProvider.Application("invissue");
            var newInvSchema = app.Schema(new ApplicationMetadataSchemaKey("newInvIssueDetail"));
            _baseIssueDisplayables = newInvSchema.Displayables;


            //TODO: Add the ability to access the base schema so that we can count the base displayables directly from a test method

            ApplicationConfiguration.TestclientName = "test_only";
            MetadataProvider.StubReset();

        }


        [TestMethod]
        public void TestAfterAndBefore1() {
            var app = MetadataProvider.Application("worklog");
            var listSchema = app.Schema(new ApplicationMetadataSchemaKey("list"));
            var displayables = listSchema.Fields;

            //Add 2 for customized fields that were added on top of the base worklog (xxx and yyy)
            //TODO: Identify and calculate the number of displayables that will automatically get added from the sr -> worklog relationship
            Assert.AreEqual(displayables.Count, _baseWorklogDisplayables.Count + 2);


            var description = displayables.FirstOrDefault(f => f.Attribute.Equals("description"));
            var descIndex = displayables.IndexOf(description);

            Assert.AreEqual("xxx", displayables[descIndex + 1].Attribute);

            var createdate = displayables.FirstOrDefault(f => f.Attribute.Equals("createdate"));
            var createdateIndex = displayables.IndexOf(createdate);

            Assert.AreEqual("yyy", displayables[createdateIndex - 1].Attribute);
        }

        [TestMethod]
        public void TestReplace() {

            var app = MetadataProvider.Application("worklog");
            var detailSchema = app.Schema(new ApplicationMetadataSchemaKey("detail", null, ClientPlatform.Web));
            var displayables = detailSchema.Fields;
            //parent fields=4 (auto-generated); customizations=2
            Assert.AreEqual(7, displayables.Count);

            Assert.IsNull(displayables.FirstOrDefault(f => f.Attribute.Equals("description")));

            Assert.IsNull(displayables.FirstOrDefault(f => f.Attribute.Equals("longdescription_.ldtext")));

            var zzzfield = displayables.FirstOrDefault(f => f.Attribute.Equals("zzz"));
            Assert.IsNotNull(zzzfield);

            var idx = displayables.IndexOf(zzzfield);
            Assert.AreEqual("www", displayables[idx + 1].Attribute);

        }

        [TestMethod]
        public void TestReplaceComposition() {

            var app = MetadataProvider.Application("incident");
            var detailSchema = app.Schema(new ApplicationMetadataSchemaKey("editdetail"));
            var compositions = detailSchema.Compositions();
            var attachmentComposition = compositions.FirstOrDefault(c => c.AssociationKey == "attachment");
            Assert.IsNull(attachmentComposition);

            var associations = detailSchema.Associations();
            Assert.IsNull(associations.FirstOrDefault(c => c.Attribute == "location"));
            //This was not replaced
            Assert.IsNotNull(associations.FirstOrDefault(c => c.Attribute == "ownergroup"));

            Assert.AreEqual(1, associations.Count(c => c.Attribute == "owner"));

            var optionFields = detailSchema.OptionFields();
            Assert.IsNull(optionFields.FirstOrDefault(c => c.Attribute == "classstructureid"));
        }



        [TestMethod]
        public void TestReplaceRendererOfAssociationInsideSection() {

            var app = MetadataProvider.Application("invissue");
            var detailSchema = app.Schema(new ApplicationMetadataSchemaKey("newInvIssueDetail"));

            var associations = detailSchema.Associations();
            var issueTo = associations.FirstOrDefault(c => c.Attribute == "issueto");
            Assert.IsNotNull(issueTo);
            Assert.AreNotEqual("lookup", issueTo.RendererType);
            Assert.AreEqual(_baseIssueDisplayables.Count, detailSchema.Displayables.Count);

        }

        [TestMethod]
        public void TestAssociationSWWEB1333() {


            var app = MetadataProvider.Application("workorder");
            var detailSchema = app.Schema(new ApplicationMetadataSchemaKey("editdetail"));

            var associations = detailSchema.Associations();
            var count = associations.Count(c => c.Attribute == "failurecode");
            Assert.AreEqual(1, count);


        }


        [TestMethod]
        public void TestPropertyCustomizationSWWEB1948() {


            var app = MetadataProvider.Application("servicerequest");
            var detailSchema = app.Schema(new ApplicationMetadataSchemaKey("editdetail"));

            Assert.AreEqual("true",detailSchema.GetProperty(ApplicationSchemaPropertiesCatalog.DetailShowTitle));



        }


        [TestMethod]
        public void TestCustomizationWithMultipleAssociationsPointingToSameTarget() {
            var matusetrans = MetadataProvider.Application("matusetrans");
            var schema = matusetrans.Schema(new ApplicationMetadataSchemaKey("detail"));
            var associations = new List<ApplicationAssociationDefinition>(schema.Associations().Where(a => a.Target == "itemnum"));
            Assert.AreEqual("sparepart_.description", associations[0].OriginalLabelField);
            Assert.AreEqual("item_.description", associations[1].OriginalLabelField);
        }

        [TestMethod]
        public void TestCommandsCustomization() {

            var app = MetadataProvider.Application("location");
            var listSchema = app.Schema(new ApplicationMetadataSchemaKey("list"));
            var commandSchema = listSchema.CommandSchema;
            Assert.IsTrue(commandSchema.HasDeclaration);
            Assert.AreEqual(1,commandSchema.ApplicationCommands.Count);
            Assert.IsTrue(commandSchema.ApplicationCommands.ContainsKey("#actions"));
            Assert.AreEqual(1,commandSchema.ApplicationCommands["#actions"].Commands.Count);


            var srApp = MetadataProvider.Application("servicerequest");
            var editSchema = srApp.Schema(new ApplicationMetadataSchemaKey("editdetail"));

            commandSchema = editSchema.CommandSchema;
            Assert.IsTrue(commandSchema.HasDeclaration);
            Assert.AreEqual(2, commandSchema.ApplicationCommands.Count);
            Assert.IsTrue(commandSchema.ApplicationCommands.ContainsKey("#actions"));
            Assert.AreEqual(3, commandSchema.ApplicationCommands["#actions"].Commands.Count);

            Assert.IsTrue(commandSchema.ApplicationCommands.ContainsKey("#detail.primary"));
            Assert.IsTrue(commandSchema.ApplicationCommands["#detail.primary"].Commands.Any(c => c.Id.Equals("customizationtest")));


        }


        [TestMethod]
        public void TestSectionReplacement() {

            if (ApplicationConfiguration.TestclientName != "test4") {
                ApplicationConfiguration.TestclientName = "test4";
                MetadataProvider.StubReset();
            }

            var app = MetadataProvider.Application("quickservicerequest");
            var newDetailSchema = app.Schema(new ApplicationMetadataSchemaKey("quicknewdetail"));
            var section =
                newDetailSchema.GetDisplayable<ApplicationSection>(typeof(ApplicationSection))
                    .FirstOrDefault(s => s.Id.EqualsIc("statussection"));

            Assert.IsNotNull(section);
            //one extra field got added
            Assert.AreEqual(4,section.Displayables.Count);
            Assert.IsTrue(section.Displayables.OfType<ApplicationFieldDefinition>().Any(a=> a.Attribute.EqualsIc("extrafield")));
            Assert.AreEqual("small",section.RendererParameters["childinputsize"]);

            
        }


        [TestMethod]
        public void ValidateTabBehavior() {

            if (ApplicationConfiguration.TestclientName != "test4") {
                ApplicationConfiguration.TestclientName = "test4";
                MetadataProvider.StubReset();
            }

            var app = MetadataProvider.Application("asset");
            var newDetailSchema = app.Schema(new ApplicationMetadataSchemaKey("detail"));
            var tabNumber =newDetailSchema.GetDisplayable<ApplicationTabDefinition>(typeof(ApplicationTabDefinition))
                    .Count(s => s.Id.EqualsIc("spareparts"));
            Assert.AreEqual(1, tabNumber);
            


        }


    }
}
