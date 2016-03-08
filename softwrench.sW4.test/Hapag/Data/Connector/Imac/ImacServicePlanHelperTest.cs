using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Persistence.WS.Ism.Entities.ISMServiceEntities;
using softwrench.sw4.Hapag.Data.Connector.Imac;

namespace softwrench.sW4.test.Hapag.Data.Connector.Imac {
    [TestClass]
    public class ImacServicePlanHelperTest {
//        [TestMethod]
//        public void TestMethod1() {
//            var result = ImacServicePlanHelper.DoLoadFromFile("HLAADDCOMH");
//            Assert.IsNotNull(result);
//            var activities = result as Activity[] ?? result.ToArray();
//            Assert.AreEqual(activities.Count(), 4);
//            var first = activities.First();
//            Assert.AreEqual("WOActivity", first.type);
//            Assert.AreEqual("Add subcomponent", first.ActionLogSummary);
//            Assert.IsNotNull(first.FlexFields);
//            Assert.AreEqual(3, first.FlexFields.Count());
//            var firstFlexField =first.FlexFields.First();
//            Assert.IsNotNull(firstFlexField.mappedTo);
//
//        }

//        [TestMethod]
//        public void Test2() {
//
//
//
//
//
//
//
//            string result = @"<Activity type=""WOActivity"">
// 	<ActionID>1</ActionID> 
// 	<ActionLog></ActionLog> 
// 	<ActionLogSummary>Add subcomponent</ActionLogSummary> 
// 	<UserID></UserID> 
// 	<FlexFields> 
// 		<FlexField mappedTo=""STATUS"" id=""0"">WAPPR</FlexField> 
// 		<FlexField mappedTo=""WOSEQUENCE"" id=""0"">10</FlexField> 
// 		<FlexField mappedTo=""OWNERGROUP"" id=""0"">V-PSB-DE-HLC-HWSUPPORT</FlexField> 
// 	</FlexFields> 
// </Activity>
// ";
//            //            var serializer = new XmlSerializer(typeof(Activity), @"http://b2b.ibm.com/schema/B2B_CDM_Incident/R2_2");
//            var serializer = new XmlSerializer(typeof (Activity));
//            var first = (Activity)serializer.Deserialize(new XmlTextReader(new StringReader(result)));
//            Assert.AreEqual("WOActivity", first.type);
//            Assert.AreEqual("Add subcomponent", first.ActionLogSummary);
//            Assert.IsNotNull(first.FlexFields);
//            Assert.AreEqual(4, first.FlexFields.Count());
//
//
//        }
    }
}
