using System;
using System.Collections.Generic;
using System.Dynamic;
using cts.commons.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace softwrench.sW4.test.Util {

    [TestClass]
    public class LoggingUtilTest {

        [TestMethod]
        public void TestParameterReplacement() {
            var ob = new ExpandoObject() as IDictionary<string, Object>;
            ob.Add("par", "x");
            ob.Add("xxx", "x");
            var result = LoggingUtil.QueryStringForLogging("from x where a = :par and b =:xxx", "teste", ob);
            Assert.AreEqual("teste: from x where a = 'x' and b ='x'", result);
        }

        [TestMethod]
        public void TestCollection() {
            var ob = new ExpandoObject() as IDictionary<string, Object>;
            ob.Add("par", new List<string>() { "p1", "p2", "p3" });
            ob.Add("xxx", "x");
            var result = LoggingUtil.QueryStringForLogging("from x where a in (:par) and b =:xxx", null, ob);
            Assert.AreEqual("from x where a in ('p1','p2','p3') and b ='x'", result);
        }


        [TestMethod]
        public void TestQuestionMark() {
            var ob = new ExpandoObject() as IDictionary<string, Object>;
            ob.Add("par", new List<string>() { "p1", "p2", "p3" });
            var result = LoggingUtil.QueryStringForLogging("from x where a in (:par) and b = ? and c = ?", null, ob, "test", "test2");
            Assert.AreEqual("from x where a in ('p1','p2','p3') and b = 'test' and c = 'test2'", result);
        }
    }
}
