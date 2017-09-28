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


        public static string RowstampMapJsonNew = @"{
            'applications':{
                'workorder':{
                      'items':[
                       {
                           'id': '100',
                            rowstamp: 1000
                        },
                        {
                            'id': '101',
                            rowstamp: 1001
                        }
                     ],    
                },
                'pastworkorder':{
                    'items':[
                       {
                            'id': '100',
                            rowstamp: 1000
                        },
                        {
                            'id': '101',
                            rowstamp: 1001
                        },
                        {
                            'id': '102',
                            rowstamp: 1002
                        }
                     ],
                }
            },
       
            compositionmap: {
               'worklog': 1000,
                'attachments': 1001,
            }
            
          }"
            ;


        public static string NonFullRowstampMapJsonNew = @"{
            'applications':{
                'workorder': '1000',
                'pastworkorder': '1001',
                'configuration': null
             },
         
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
            var result = ClientStateJsonConverter.ConvertJSONToDict(ob)[0].ClientState;
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.ContainsKey("100"));
            Assert.AreEqual("1000", result["100"]);
        }


        [TestMethod]
        public void TestJsonConversionNew() {
            var ob = JObject.Parse(RowstampMapJsonNew);
            var dict = ClientStateJsonConverter.ConvertJSONToDict(ob);

            var app1 = dict[0];
            var result = app1.ClientState;
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("workorder", app1.ApplicationName);

            app1 = dict[1];
            result = app1.ClientState;
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("pastworkorder", app1.ApplicationName);

            Assert.IsTrue(result.ContainsKey("100"));
            Assert.AreEqual("1000", result["100"]);
        }


        [TestMethod]
        public void TestJsonConversionNewNonFull() {
            var ob = JObject.Parse(NonFullRowstampMapJsonNew);
            var dict = ClientStateJsonConverter.ConvertJSONToDict(ob);

            var app1 = dict[0];
            var result = app1.ClientState;
            Assert.AreEqual(result.Count,0);
            Assert.AreEqual("workorder", app1.ApplicationName);
            Assert.AreEqual("1000", app1.MaxRowstamp);

            app1 = dict[1];
            result = app1.ClientState;
            Assert.AreEqual(result.Count, 0);
            Assert.AreEqual("pastworkorder", app1.ApplicationName);
            Assert.AreEqual("1001", app1.MaxRowstamp);
            
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
