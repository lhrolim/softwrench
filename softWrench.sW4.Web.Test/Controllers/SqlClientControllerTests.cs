using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Web.Controllers.SqlClient;
using Moq;
using softWrench.sW4.SqlClient;
using cts.commons.simpleinjector;
using cts.commons.persistence;
using System.Collections.Generic;
using System.Dynamic;
using softwrench.sW4.TestBase;

namespace softWrench.sW4.Web.Test.Controllers {
    [TestClass]
    public class SqlClientControllerTests : BaseMetadataTest {
        [TestMethod]
        public void SqlClientTest() {
            var target = new SqlClientController();

            var result = target.SqlClient();

            Assert.AreEqual("/Content/Controller/SqlClient.html", result.RedirectURL);
        }

        [TestMethod]
        public void ExecuteQuery_swdb_non_crud_Test() {
            var sqlClientMock = new Mock<ISqlClient>();
            var swdbMock = new Mock<ISWDBHibernateDAO>();
            var maximodbMock = new Mock<IMaximoHibernateDAO>();

            sqlClientMock.Setup(a => a.IsDefinitionOrManipulation(It.IsAny<string>()))
               .Returns(() => false);

            swdbMock.Setup(x => x.FindByNativeQuery(It.IsAny<string>(), It.IsAny<ExpandoObject>(), It.IsAny<IPaginationData>(), It.IsAny<string>()))
                .Returns(() => new List<dynamic>() { "record 1", "record 2" });

            var scanner = new TestSimpleInjectorScanner();
            scanner.ResgisterSingletonMock<ISWDBHibernateDAO>(swdbMock);
            scanner.ResgisterSingletonMock<IMaximoHibernateDAO>(maximodbMock);
            scanner.InitDIController();

            var injector = new SimpleInjectorGenericFactory(scanner.Container);

            var target = new SqlClientController();
            var result = target.ExecuteQuery("select hello from world", "swdb", 10);

            Assert.AreEqual("2 records(s) returned", result.ExecutionMessage);
            Assert.AreEqual(2, result.ResultSet.Count);
            Assert.IsFalse(result.HasErrors);
        }

        [TestMethod]
        public void ExecuteQuery_swdb_crud_Test() {
            var sqlClientMock = new Mock<ISqlClient>();
            var swdbMock = new Mock<ISWDBHibernateDAO>();
            var maximodbMock = new Mock<IMaximoHibernateDAO>();

            swdbMock.Setup(x => x.ExecuteSql(It.IsAny<string>(), null))
                .Returns(() => 2);

            var scanner = new TestSimpleInjectorScanner();
            scanner.ResgisterSingletonMock<ISWDBHibernateDAO>(swdbMock);
            scanner.ResgisterSingletonMock<IMaximoHibernateDAO>(maximodbMock);
            scanner.InitDIController();

            var injector = new SimpleInjectorGenericFactory(scanner.Container);

            var target = new SqlClientController();
            var result = target.ExecuteQuery("insert into hello(message) values('hi')", "swdb", 10);

            Assert.AreEqual("2 records(s) affected", result.ExecutionMessage);
            Assert.IsFalse(result.HasErrors);
            Assert.IsNull(result.ResultSet);
        }

        [TestMethod]
        public void ExecuteQuery_invalid_parms_Test() {
            var sqlClientMock = new Mock<ISqlClient>();
            var swdbMock = new Mock<ISWDBHibernateDAO>();
            var maximodbMock = new Mock<IMaximoHibernateDAO>();

            var scanner = new TestSimpleInjectorScanner();
            scanner.ResgisterSingletonMock<ISWDBHibernateDAO>(swdbMock);
            scanner.ResgisterSingletonMock<IMaximoHibernateDAO>(maximodbMock);
            scanner.InitDIController();

            var injector = new SimpleInjectorGenericFactory(scanner.Container);

            var target = new SqlClientController();
            var result = target.ExecuteQuery(null, null, 0);

            Assert.AreEqual("The sql query or the datasource cannot be empty.", result.ExecutionMessage);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNull(result.ResultSet);
        }

        [TestMethod]
        public void ExecuteQuery_invalid_query_Test() {
            var sqlClientMock = new Mock<ISqlClient>();
            var swdbMock = new Mock<ISWDBHibernateDAO>();
            var maximodbMock = new Mock<IMaximoHibernateDAO>();

            sqlClientMock.Setup(a => a.IsDefinitionOrManipulation(It.IsAny<string>()))
               .Returns(() => false);

            swdbMock.Setup(x => x.FindByNativeQuery(It.IsAny<string>(), It.IsAny<ExpandoObject>(), It.IsAny<IPaginationData>(), It.IsAny<string>()))
                .Throws(new Exception("Invalid Query", new Exception("Object 'Hello1 doesnt exist'")));

            var scanner = new TestSimpleInjectorScanner();
            scanner.ResgisterSingletonMock<ISWDBHibernateDAO>(swdbMock);
            scanner.ResgisterSingletonMock<IMaximoHibernateDAO>(maximodbMock);
            scanner.InitDIController();

            var injector = new SimpleInjectorGenericFactory(scanner.Container);

            var target = new SqlClientController();
            var result = target.ExecuteQuery("select hello1 from world", "swdb", 10);

            Assert.AreEqual("Invalid Query : Object 'Hello1 doesnt exist'", result.ExecutionMessage);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNull(result.ResultSet);
        }
    }
}
