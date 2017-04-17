using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;

namespace softwrench.sW4.test.Data.Persistence.Relational.Cache {
    /// <summary>
    /// Summary description for RedisLookupDTOTest
    /// </summary>
    [TestClass]
    public class RedisLookupDTOTest {

        [TestMethod]
        public void TestLinearCombination() {

            var schema = new ApplicationSchemaDefinition {
                SchemaId = "list",
                ApplicationName = "asset"
            };

            var lookup = new RedisLookupDTO {
                Schema = schema,
                IsOffline = true,
                ExtraKeys = new Dictionary<string, object>{
                    {"orgid", "EAGLESA" },
                    {"Siteid", "1801" },
                    {"facilities",new List<string>(){"AES","ACS"}},
                    {"fakeprop",new List<string>(){"1"}},
                    {"fakeprop2",new List<string>(){"a","b","c"}}
                }
            };

            var keys = new List<string>(lookup.BuildKeys());
            Assert.AreEqual(7, keys.Count());

            Assert.AreEqual("application=asset;facilities=ACS,AES;fakeprop=1;fakeprop2=a,b,c;offline=true;orgid=EAGLESA;schemaid=list;siteid=1801", keys[0]);
            Assert.AreEqual("application=asset;facilities=ACS;fakeprop=1;fakeprop2=a;offline=true;orgid=EAGLESA;schemaid=list;siteid=1801", keys[1]);
            Assert.AreEqual("application=asset;facilities=ACS;fakeprop=1;fakeprop2=b;offline=true;orgid=EAGLESA;schemaid=list;siteid=1801", keys[2]);
            Assert.AreEqual("application=asset;facilities=ACS;fakeprop=1;fakeprop2=c;offline=true;orgid=EAGLESA;schemaid=list;siteid=1801", keys[3]);
            Assert.AreEqual("application=asset;facilities=AES;fakeprop=1;fakeprop2=a;offline=true;orgid=EAGLESA;schemaid=list;siteid=1801", keys[4]);
            Assert.AreEqual("application=asset;facilities=AES;fakeprop=1;fakeprop2=b;offline=true;orgid=EAGLESA;schemaid=list;siteid=1801", keys[5]);
            Assert.AreEqual("application=asset;facilities=AES;fakeprop=1;fakeprop2=c;offline=true;orgid=EAGLESA;schemaid=list;siteid=1801", keys[6]);


        }


        [TestMethod]
        public void TestSimpleScenario() {
            var schema = new ApplicationSchemaDefinition {
                SchemaId = "list",
                ApplicationName = "asset"
            };

            var lookup = new RedisLookupDTO {
                Schema = schema,
                IsOffline = true,
                ExtraKeys = new Dictionary<string, object>{
                    {"orgid", "EAGLESA" },
                    {"Siteid", "1801" },
                    {"facilities",new List<string>(){"ACS"}},
                }
            };

            var keys = new List<string>(lookup.BuildKeys());
            Assert.AreEqual(1, keys.Count());

            Assert.AreEqual("application=asset;facilities=ACS;offline=true;orgid=EAGLESA;schemaid=list;siteid=1801", keys[0]);



        }

    }
}
