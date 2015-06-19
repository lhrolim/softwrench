using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data.Persistence.WS.Applications.InvIssue;

namespace softwrench.sW4.test.Data.Persistence.WS.Applications {

    [TestClass]
    public class InvIssueBatchConverterTest {

        private string datamap = @"{'mode':'batch',
'matusetransid':null,
'issuetype':'ISSUE',
'enterby':'swadmin',
'issueto':'ADAMS',
'siteid':'BEDFORD',
'storeloc':null,
'itemnum':'10112',
'binnum':'D-9-4',
'binbalances_.lotnum':null,

'inventory_.item_.itemtype':'ITEM',
'rotassetnum':null,
'inventory_.item_.rotating':0,
'inventory_.item_.iskit':0,
'binbalances_.curbal':12,
'quantity':1,
'inventory_.issueunit':'EACH',
'unitcost':13.5,
'inventory_.costtype':'AVERAGE',
'refwo':'1001',
'location':'SHIPPING',
'assetnum':'12600',
'gldebitacct':null,
'undefined':1,
'#batchitem_':[
{'storeloc':'CENTRAL',
'storeroom_':{'description':'Central Storeroom'},
'extrafields':{'storeroom_':{'description':'Central Storeroom'},
'storeroom_.description':'Central Storeroom',
'binbalances_':{'curbal':7,
'lotnum':null,
'binnum':'B-7-3'},
'binbalances_.curbal':7,
'binbalances_.lotnum':null,
'binbalances_.binnum':'B-7-3'},
'storeroom_.description':'Central Storeroom',
'binnum':'B-7-3',
'binbalances_':{'curbal':7,
'lotnum':null,
'binnum':'B-7-3'},
'binbalances_.curbal':7,
'binbalances_.lotnum':null,
'binbalances_.binnum':'B-7-3',
'lotnum':null,
'#curbal':7,
'curbal':7,
'quantity':'4'},
{'storeloc':'CENTRAL2',
'storeroom_':{'description':'Central Storeroom'},
'extrafields':{'storeroom_':{'description':'Central Storeroom'},
'storeroom_.description':'Central Storeroom',
'binbalances_':{'curbal':7,
'lotnum':null,
'binnum':'B-7-3'},
'binbalances_.curbal':7,
'binbalances_.lotnum':null,
'binbalances_.binnum':'B-7-3'},
'storeroom_.description':'Central Storeroom',
'binnum':'B-7-4',
'binbalances_':{'curbal':7,
'lotnum':null,
'binnum':'B-7-3'},
'binbalances_.curbal':7,
'binbalances_.lotnum':null,
'binbalances_.binnum':'B-7-3',
'lotnum':null,
'#curbal':7,
'curbal':7,
'quantity':'5'},
],

'lotnum':null,
'#curbal':12,
'curbal':12}";


        [TestMethod]
        public void TestBreakingIntoLines() {
            var arr = new InvIssueBatchConverter().BreakIntoRows(JObject.Parse(datamap));
            Assert.AreEqual(2, arr.Count);
            Assert.AreEqual("CENTRAL", arr[0]["storeloc"].Value<string>());
            Assert.AreEqual("CENTRAL2", arr[1]["storeloc"].Value<string>());
        }
    }
}
