﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Relationships.Associations;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Metadata {
    [TestClass]
    public class CustomizationTest {

        private static IList<ApplicationFieldDefinition> _baseWorklogDisplayables;

        private static List<IApplicationDisplayable> _baseIssueDisplayables;

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "otb";
            MetadataProvider.StubReset();

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
            //Subtract 1 field that will be automatically added to OTB's base worklog from having a relationship from SR -> Worklog
            //(test_only does not include servicerequests, and therefore does not have a relationship from SR -> Worklog)
            //TODO: Identify and calculate the number of displayables that will automatically get added from the sr -> worklog relationship
            Assert.AreEqual(displayables.Count, _baseWorklogDisplayables.Count + 2 - 1);


            var description = displayables.FirstOrDefault(f => f.Attribute.Equals("description"));
            var descIndex = displayables.IndexOf(description);

            Assert.AreEqual("xxx", displayables[descIndex+1].Attribute);

            var createdate = displayables.FirstOrDefault(f => f.Attribute.Equals("createdate"));
            var createdateIndex = displayables.IndexOf(createdate);

            Assert.AreEqual("yyy", displayables[createdateIndex-1].Attribute);
        }

        [TestMethod]
        public void TestReplace() {

            var app = MetadataProvider.Application("worklog");
            var detailSchema = app.Schema(new ApplicationMetadataSchemaKey("detail"));
            var displayables = detailSchema.Fields;
            //parent fields=3 (auto-generated); customizations=2
            Assert.AreEqual(5, displayables.Count);

            Assert.IsNull(displayables.FirstOrDefault(f => f.Attribute.Equals("description")));

            Assert.IsNull(displayables.FirstOrDefault(f => f.Attribute.Equals("longdescription_.ldtext")));

            var zzzfield= displayables.FirstOrDefault(f => f.Attribute.Equals("zzz"));
            Assert.IsNotNull(zzzfield);

            var idx = displayables.IndexOf(zzzfield);
            Assert.AreEqual("www",displayables[idx+1].Attribute);

        }

        [TestMethod]
        public void TestReplaceComposition() {

            var app = MetadataProvider.Application("incident");
            var detailSchema = app.Schema(new ApplicationMetadataSchemaKey("detail"));
            var compositions = detailSchema.Compositions;
            var attachmentComposition = compositions.FirstOrDefault(c => c.AssociationKey == "attachment");
            Assert.IsNull(attachmentComposition);

            var associations = detailSchema.Associations;
            Assert.IsNull(associations.FirstOrDefault(c => c.Attribute == "location"));
            //This was not replaced
            Assert.IsNotNull(associations.FirstOrDefault(c => c.Attribute == "ownergroup"));

            Assert.AreEqual(1,associations.Count(c => c.Attribute == "owner"));

            var optionFields =detailSchema.OptionFields;
            Assert.IsNull(optionFields.FirstOrDefault(c => c.Attribute == "classstructureid"));
        }



        [TestMethod]
        public void TestReplaceRendererOfAssociationInsideSection() {

            var app = MetadataProvider.Application("invissue");
            var detailSchema = app.Schema(new ApplicationMetadataSchemaKey("newInvIssueDetail"));
            
            var associations = detailSchema.Associations;
            var issueTo = associations.FirstOrDefault(c => c.Attribute == "issueto");
            Assert.IsNotNull(issueTo);
            Assert.AreNotEqual("lookup",issueTo.RendererType);
            Assert.AreEqual(_baseIssueDisplayables.Count,detailSchema.Displayables.Count);
            
        }

    }
}
