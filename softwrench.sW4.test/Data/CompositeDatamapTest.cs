using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Data;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Entities;

namespace softwrench.sW4.test.Data {
    [TestClass]
    public class CompositeDatamapTest {


        [TestMethod]
        public void CompositeDataMapConstructorTest() {

            var list = new List<AttributeHolder>{
                AttributeHolder.TestInstance(
                    new Dictionary<string, object>{
                    {"k1","v11"},
                    {"k2",""},
                    {"k3","v13"},
                    {"k4",null }
                }),

                AttributeHolder.TestInstance(
                    new Dictionary<string, object>{
                    {"k1","v21"},
                    {"k2","v22"},
                    {"k3",""}
                })
            };

            var result = new CompositeDatamap(list);
            Assert.AreEqual(result.GetAttribute("k1"), "v11,v21");
            Assert.AreEqual(result.GetAttribute("k2"), "v22");
        }
    }
}
