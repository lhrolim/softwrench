using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Data;

namespace softwrench.sW4.test.Data {
    [TestClass]
    public class JsonXmlProblemTest {
        private string expected = @"{
  ""xml"": {
    ""content"": ""<xml>test</xml>"",
    ""type"": ""xml""
  },
  ""json"": {
    ""content"": ""{\""test\"":{\""prop1\"":\""abc\""}}"",
    ""type"": ""json""
  }
}";

        private string expectedJson =
@"""json"": {
    ""content"": ""{\""test\"":{\""prop1\"":\""abc\""}}"",
    ""type"": ""json""
 }";

        private string expectedXml =

@" ""xml"": {
    ""content"": ""<xml>test</xml>"",
    ""type"": ""xml""
 }";

        [TestMethod]
        public void TestJsonGeneratedTheRightWay() {
            var result = new JSonXmlProblemData("<xml>test</xml>", JObject.Parse("{'test': {'prop1': 'abc'}}")).Serialize();
            Assert.AreEqual(expected, result);
        }

        // TODO - Fix the test
        //[TestMethod]
        //public void TestDeserialization() {
        //    var result = JSonXmlProblemData.Deserialize(expected);
        //    Assert.AreEqual(expectedJson, result.Json);
        //    Assert.AreEqual(expectedXml, result.Xml);
        //}
    }
}
