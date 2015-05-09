using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using softwrench.sw4.offlineserver.services;

namespace softwrench.sW4.test.offline {
    [TestClass]
    public class SynchronizationManagerTest {


        public static string RowstampMapJson = @"{
            'items': 
             [{
                    'id': '100',
                    rowstamp: '1000'
                },{
                    'id': '101',
                    rowstamp: '1001'
                }
             ],
            compositionmap: {
                'worklog': '1000',
                'attachments': '1001'
            }
          }"
            ;

        [TestMethod]
        public void TestJsonConversion() {
            var ob = JObject.Parse(RowstampMapJson);
            var result = SynchronizationManager.ConvertJSONToDict(ob);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("100"));
            Assert.AreEqual("1000", result["100"]);
        }

        [TestMethod]
        public void TestCompositionConversion() {
            var ob = JObject.Parse(RowstampMapJson);
            var result = SynchronizationManager.GetCompositionRowstampsDict(ob);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("worklog"));
            Assert.AreEqual(1000, result["worklog"]);
        }
    }
}
