using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softWrench.sW4.Util;
using softwrench.sW4.TestBase;

namespace softwrench.sW4.test.Metadata {

    [TestClass]
    public class AbstractSchemaTest : BaseMetadataTest {

        [TestInitialize]
        public void Init() {
            if (ApplicationConfiguration.TestclientName != "test_only") {
                ApplicationConfiguration.TestclientName = "test_only";
                MetadataProvider.StubReset();
            }
        }

        [TestMethod]
        public void CheckFieldsOrderOrder()
        {
            var appl = MetadataProvider.Application("workorder");
            var new1 = appl.Schemas().First(r => Equals(r.Key, new ApplicationMetadataSchemaKey("new1"))).Value;

            Assert.AreEqual(SchemaMode.input, new1.Mode);
            Assert.AreEqual(SchemaStereotype.Detail,new1.Stereotype);
            Assert.IsTrue(new1.Properties.ContainsKey("prop1"));
            
            var displayables = new1.Displayables;
            Assert.AreEqual(9, displayables.Count);
            CheckAttributeName(displayables,0,"siteid");
            CheckAttributeName(displayables,1,"woclass");
            CheckAttributeName(displayables,2,"wonum");
            CheckAttributeName(displayables,3,"status");
            CheckAttributeName(displayables,4,"wopriority");
            CheckAttributeName(displayables,5,"owner");
            CheckAttributeName(displayables,6,"location");
            

        }

        [TestMethod]
        public void CheckFieldsOrderOrder2() {
            var appl = MetadataProvider.Application("workorder");
            var new1 = appl.Schemas().First(r => Equals(r.Key, new ApplicationMetadataSchemaKey("new2"))).Value;

            Assert.AreEqual(SchemaMode.input, new1.Mode);
            Assert.AreEqual(SchemaStereotype.Detail, new1.Stereotype);
            Assert.IsNotNull(new1.Properties.FirstOrDefault(p => p.Key == ApplicationSchemaPropertiesCatalog.ListClickMode));

            var displayables = new1.Displayables;
            Assert.AreEqual(9, displayables.Count);
            
            CheckAttributeName(displayables, 1, "wonum");
            CheckAttributeName(displayables, 2, "status");
            CheckAttributeName(displayables, 3, "owner");
            CheckAttributeName(displayables, 5, "location");
            CheckAttributeName(displayables, 6, "siteid");
            CheckAttributeName(displayables, 7, "woclass");

        }

        private static void CheckAttributeName(IList<IApplicationDisplayable> displayables,int index,string name) {
            var f = displayables[index] as ApplicationFieldDefinition;
            if (f == null) {
                throw new AssertFailedException("field should be an applicaitonField");
            }
            Assert.AreEqual(name, f.Attribute);
        }
    }
}
