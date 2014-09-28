using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.Hapag.Security;

namespace softwrench.sW4.test.Hapag.Security {
    [TestClass]
    public class HlagGroupedLocationTest {
        [TestMethod]
        public void TestQueryGenerator()
        {
            var l=new HlagGroupedLocation("any",new HashSet<string> {"a","b","c"},true);
            var query =l.CostCentersForQuery("asset.location");
            Assert.AreEqual("(asset.location like '%a%' or asset.location like '%b%' or asset.location like '%c%')", query);
        }
    }
}
