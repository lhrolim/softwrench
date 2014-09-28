using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.EL;
using softWrench.sW4.Data.EL.ToString;

namespace softwrench.sW4.test
{
    [TestClass]
    public class ToStringElParserTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            ToStringExpression result = ToStringELParser.ParseExpression("valor @description");
            Assert.AreEqual("valor {0}",result.ConstExpression);
            Assert.AreEqual(1,result.FieldNames.Count);
            Assert.AreEqual("description", result.FieldNames[0]);
        }

        [TestMethod]
        public void TestMethod2()
        {
            ToStringExpression result = ToStringELParser.ParseExpression("valor @description e unidade= @unidade");
            Assert.AreEqual("valor {0} e unidade= {1}", result.ConstExpression);
            Assert.AreEqual(2, result.FieldNames.Count);
            Assert.AreEqual("description", result.FieldNames[0]);
            Assert.AreEqual("unidade", result.FieldNames[1]);
        }


        [TestMethod]
        public void TestMethod3_only_var()
        {
            ToStringExpression result = ToStringELParser.ParseExpression("@description");
            Assert.AreEqual("{0}", result.ConstExpression);
            Assert.AreEqual(1, result.FieldNames.Count);
            Assert.AreEqual("description", result.FieldNames[0]);
        }
    }
}
