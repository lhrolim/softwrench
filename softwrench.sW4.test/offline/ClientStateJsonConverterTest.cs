using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using softwrench.sw4.offlineserver.services.util;

namespace softwrench.sW4.test.offline {
    [TestClass]
    public class ClientStateJsonConverterTest {


        public static string RowstampMapJson = @"{
            'items': 
             [
               {
                    'id': '100',
                    rowstamp: 1000
                },
                {
                    'id': '101',
                    rowstamp: 1001
                }
             ],
            compositionmap: {
               'worklog': 1000,
                'attachments': 1001,
            }
    
          }"
            ;

        public static string AssociationMapJson = @"{
                'associationmap': {
                    'asset': {
                        'maximorowstamp': 12500,
                        'whereclausehash': 'hashoflatestappliedwhereclause',
                        'syncschemahash': 'baadfasdfasdfa'
                    },

                    'problem': {
                        'maximorowstamp': 12000,
                        'whereclausehash': 'hashoflatestappliedwhereclause',
                        'syncschemahash': 'baadfasdfasdfa'
                    }
                }
            }";


        public static string AssociationMapUidJson = @"{
                'associationmap': {
                    'asset': {
                        'maximouid': 12500,
                        'whereclausehash': 'hashoflatestappliedwhereclause',
                        'syncschemahash': 'baadfasdfasdfa'
                    },

                    'problem': {
                        'maximouid': 12000,
                        'whereclausehash': 'hashoflatestappliedwhereclause',
                        'syncschemahash': 'baadfasdfasdfa'
                    }
                }
            }";

        [TestMethod]
        public void TestJsonConversion() {
            var ob = JObject.Parse(RowstampMapJson);
            var result = ClientStateJsonConverter.ConvertJSONToDict(ob).ClientState;
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("100"));
            Assert.AreEqual("1000", result["100"]);
        }

        [TestMethod]
        public void TestCompositionConversion() {
            var ob = JObject.Parse(RowstampMapJson);
            var result = ClientStateJsonConverter.GetCompositionRowstampsDict(ob);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("worklog"));
            Assert.AreEqual(1000, result["worklog"]);
        }

        [TestMethod]
        public void TestAssociationConversion() {
            var ob = JObject.Parse(AssociationMapJson);
            var result = ClientStateJsonConverter.GetAssociationRowstampDict(ob);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("asset"));
            Assert.IsTrue(result.ContainsKey("problem"));
            var assetMap = result["asset"];
            Assert.AreEqual("12500", assetMap.MaxRowstamp);
        }

        [TestMethod]
        public void TestAssociationConversionUid() {
            var ob = JObject.Parse(AssociationMapUidJson);
            var result = ClientStateJsonConverter.GetAssociationRowstampDict(ob);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("asset"));
            Assert.IsTrue(result.ContainsKey("problem"));
            var assetMap = result["asset"];
            Assert.AreEqual("12500", assetMap.MaxUid);
        }
    }
}
