using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.TestBase;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Mapping;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.MappingTest {
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


            var daoResult = new List<Mapping>
            {
                Mapping.TestValue("key","1","a"),
                Mapping.TestValue("key","2","b"),
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


            var daoResult = new List<Mapping>
            {
                Mapping.TestValue("key","1","a,b,c"),
                Mapping.TestValue("key","2","d"),
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


            var daoResult = new List<Mapping>
            {
                Mapping.TestValue("key","1&2","a,b,c",1),
                Mapping.TestValue("key","@default","d"),
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


            var daoResult = new List<Mapping>
            {
                Mapping.TestValue("key","1,2","a,b,c",1),
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
