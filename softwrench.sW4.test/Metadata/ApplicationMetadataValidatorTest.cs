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
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.Validator;
using softWrench.sW4.Security.Context;

namespace softwrench.sW4.test.Metadata {

    [TestClass]
    public class ApplicationMetadataValidatorTest {


        [TestMethod]
        public void TestMetadataValidation() {
            foreach (var clientName in TestUtil.ClientNames()) {
                if (clientName == "pae" || clientName == "hapag") {
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

                var scanner = new TestSimpleInjectorScanner();
                scanner.ResgisterSingletonMock<ISWDBHibernateDAO>(swdbMock);
                scanner.ResgisterSingletonMock<IMaximoHibernateDAO>(maximodbMock);
                scanner.ResgisterSingletonMock<IContextLookuper>(contextLookuperMock);

                scanner.InitDIController();

                var injector = new SimpleInjectorGenericFactory(scanner.Container);

                var dataSetProvider = DataSetProvider.GetInstance();
                dataSetProvider.Clear();
                dataSetProvider.HandleEvent(new ApplicationStartedEvent());

                var validator = new ApplicationMetadataValidator();
                validator.HandleEvent(new ApplicationStartedEvent());
            }

        }
    }
}
