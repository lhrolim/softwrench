using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softWrench.sW4.Mapping;

namespace softwrench.sW4.test.Mapping {
    [TestClass]
    public class MappingResolverTest {

        private Mock<ISWDBHibernateDAO> _swdbDAO;
        private MappingResolver _mappingResolver;

        [TestInitialize]
        public void Init() {
            _swdbDAO = new Mock<ISWDBHibernateDAO>();
            _mappingResolver = new MappingResolver(_swdbDAO.Object);
        }


        [TestMethod]
        public void BasicTest() {


            var daoResult = new List<softWrench.sW4.Mapping.Mapping>
            {
                softWrench.sW4.Mapping.Mapping.TestValue("1","a"),
                softWrench.sW4.Mapping.Mapping.TestValue("2","b"),
            };

            _swdbDAO.Setup(a => a.FindByQuery<softWrench.sW4.Mapping.Mapping>(softWrench.sW4.Mapping.Mapping.ByKey, "key")).Returns(daoResult);
            var result = _mappingResolver.Resolve("key", new List<string>() { "1", "2" });
            Assert.AreEqual(2, result.Count());

            Assert.AreEqual(result[0], "a");
            Assert.AreEqual(result[1], "b");


            _swdbDAO.Verify();

            //test caching, no DAO call
            result = _mappingResolver.Resolve("key", new List<string>() { "1", "2" });
            Assert.AreEqual(2, result.Count());

            Assert.AreEqual(result[0], "a");
            Assert.AreEqual(result[1], "b");


        }


        [TestMethod]
        public void MultipleConditionTest() {


            var daoResult = new List<softWrench.sW4.Mapping.Mapping>
            {
                softWrench.sW4.Mapping.Mapping.TestValue("1","a,b,c"),
                softWrench.sW4.Mapping.Mapping.TestValue("2","d"),
            };

            _swdbDAO.Setup(a => a.FindByQuery<softWrench.sW4.Mapping.Mapping>(softWrench.sW4.Mapping.Mapping.ByKey, "key")).Returns(daoResult);
            var result = _mappingResolver.Resolve("key", new List<string>() { "1", "2" });
            Assert.AreEqual(4, result.Count());

            Assert.AreEqual(result[0], "a");
            Assert.AreEqual(result[1], "b");
            Assert.AreEqual(result[2], "c");
            Assert.AreEqual(result[3], "d");
            _swdbDAO.Verify();

        }


        [TestMethod]
        public void AndConditionTest() {


            var daoResult = new List<softWrench.sW4.Mapping.Mapping>
            {
                softWrench.sW4.Mapping.Mapping.TestValue("1&2","a,b,c",1),
                softWrench.sW4.Mapping.Mapping.TestValue("@default","d"),
            };

            _swdbDAO.Setup(a => a.FindByQuery<softWrench.sW4.Mapping.Mapping>(softWrench.sW4.Mapping.Mapping.ByKey, "key")).Returns(daoResult);
            var result = _mappingResolver.Resolve("key", new List<string>() { "1", "2" });
            Assert.AreEqual(4, result.Count());

            Assert.AreEqual(result[0], "a");
            Assert.AreEqual(result[1], "b");
            Assert.AreEqual(result[2], "c");
            Assert.AreEqual(result[3], "d");
            _swdbDAO.Verify();

        }


        [TestMethod]
        public void MultipleOrigins() {


            var daoResult = new List<softWrench.sW4.Mapping.Mapping>
            {
                softWrench.sW4.Mapping.Mapping.TestValue("1,2","a,b,c",1),
            };

            _swdbDAO.Setup(a => a.FindByQuery<softWrench.sW4.Mapping.Mapping>(softWrench.sW4.Mapping.Mapping.ByKey, "key")).Returns(daoResult);
            var result = _mappingResolver.Resolve("key", new List<string>() { "2" });
            Assert.AreEqual(3, result.Count());

            Assert.AreEqual(result[0], "a");
            Assert.AreEqual(result[1], "b");
            Assert.AreEqual(result[2], "c");
            _swdbDAO.Verify();

        }
    }
}
