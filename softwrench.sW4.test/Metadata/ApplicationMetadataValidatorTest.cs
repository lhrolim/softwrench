using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.test.Util;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using System.Diagnostics;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using Moq;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.Validator;
using softWrench.sW4.Security.Context;
using softwrench.sW4.TestBase;
using softWrench.sW4.Dynamic;
using softWrench.sW4.Dynamic.Services;

namespace softwrench.sW4.test.Metadata {

    [TestClass]
    public class ApplicationMetadataValidatorTest : BaseMetadataTest {


        [TestMethod]
        public void TestMetadataValidation() {
            foreach (var clientName in TestUtil.ClientNames()) {
                if (clientName == "hapag") {
                    // pae -> maximo 7.1
                    // hapag -> too alien
                    continue;
                }

                Debug.WriteLine(clientName);
                ApplicationConfiguration.TestclientName = clientName;

                ApplicationMetadataValidator.Clear();

                try {
                    MetadataProvider.StubReset();
                } catch (Exception e) {
                    throw new Exception("client {0} failed".Fmt(clientName), e);
                }

                var swdbMock = new Mock<ISWDBHibernateDAO>();
                var maximodbMock = new Mock<IMaximoHibernateDAO>();
                var contextLookuperMock = new Mock<IContextLookuper>();
                var whereClauseFacadeMock = new Mock<IWhereClauseFacade>();
                var dynComponentEmailer = new Mock<IDynComponentEmailer>();

                var scanner = new TestSimpleInjectorScanner();
                scanner.ResgisterSingletonMock<ISWDBHibernateDAO>(swdbMock);
                scanner.ResgisterSingletonMock<IMaximoHibernateDAO>(maximodbMock);
                scanner.ResgisterSingletonMock<IContextLookuper>(contextLookuperMock);
                scanner.ResgisterSingletonMock<IWhereClauseFacade>(whereClauseFacadeMock);
                scanner.ResgisterSingletonMock<IDynComponentEmailer>(dynComponentEmailer);

                scanner.InitDIController();

                var dataSetProvider = DataSetProvider.GetInstance();
                dataSetProvider.Clear();
                dataSetProvider.HandleEvent(new ApplicationStartedEvent());

                var validator = new ApplicationMetadataValidator();
                validator.HandleEvent(new ApplicationStartedEvent());
            }

        }
    }
}
