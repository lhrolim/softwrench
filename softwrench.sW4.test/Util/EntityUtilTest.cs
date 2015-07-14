using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Entities;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Util {

    [TestClass]
    public class EntityUtilTest {

        [TestMethod]
        public void TestEvalQueryNoExpression() {
            var result = EntityUtil.EvaluateQuery("metername = 'ASSETNOISE'", Entity.TestInstance(new Dictionary<string, object>() { { "metername", "ASSETNOISE" } }));
            Assert.AreEqual("metername = 'ASSETNOISE'", result);
        }

        [TestMethod]
        public void TestEvalQuery() {
            var result = EntityUtil.EvaluateQuery("metername = !@metername", Entity.TestInstance(new Dictionary<string, object>() { { "metername", "ASSETNOISE" } }));
            Assert.AreEqual("metername = 'ASSETNOISE'", result);

            var resultInt = EntityUtil.EvaluateQuery("metername = !@metername", Entity.TestInstance(new Dictionary<string, object>() { { "metername", 10 } }));
            Assert.AreEqual("metername = 10", resultInt);

        }

        [TestMethod]
        public void TestEvalQueryNull() {
            var resultNull = EntityUtil.EvaluateQuery("metername = !@metername", Entity.TestInstance(new Dictionary<string, object>() { { "metername", null } }));
            Assert.AreEqual("metername is null ", resultNull);
        }
    }
}
