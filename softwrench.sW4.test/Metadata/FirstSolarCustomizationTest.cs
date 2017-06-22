using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.TestBase;
using softWrench.sW4.Metadata;

namespace softwrench.sW4.test.Metadata {

    public class FirstSolarCustomizationTest : BaseMetadataTest{



        [TestMethod]
        public void TestAfterAndBefore1() {
//            var app = MetadataProvider.Application("_workpackage");
//            var outputSchema = app.Schema(new ApplicationMetadataSchemaKey("list"));
//            var displayables = listSchema.Fields;
//
//            //Add 2 for customized fields that were added on top of the base worklog (xxx and yyy)
//            //TODO: Identify and calculate the number of displayables that will automatically get added from the sr -> worklog relationship
//            Assert.AreEqual(displayables.Count, _baseWorklogDisplayables.Count + 2);
//
//
//            var description = displayables.FirstOrDefault(f => f.Attribute.Equals("description"));
//            var descIndex = displayables.IndexOf(description);
//
//            Assert.AreEqual("xxx", displayables[descIndex + 1].Attribute);
//
//            var createdate = displayables.FirstOrDefault(f => f.Attribute.Equals("createdate"));
//            var createdateIndex = displayables.IndexOf(createdate);
//
//            Assert.AreEqual("yyy", displayables[createdateIndex - 1].Attribute);
        }


        public virtual string GetClientName() {
            return "firstsolar";
        }

    }
}
