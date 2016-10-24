﻿using System;
using System.IO;
using System.Linq.Expressions;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.services.setup;
using softwrench.sW4.test.Util;
using softwrench.sW4.TestBase;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Person;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Dynamic;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Persistence.DataSet.Commons {
    [TestClass]
    public class BasePersonDataSetTest : BaseOtbMetadataTest {

        private ApplicationMetadata _applicationMetadata;
        private ApplicationMetadata _udpdateMetadata;


        private readonly Mock<ISWDBHibernateDAO> _swdbMock = TestUtil.CreateMock<ISWDBHibernateDAO>();
        private readonly Mock<MaximoConnectorEngine> _maximoEngine = TestUtil.CreateMock<MaximoConnectorEngine>();
        private readonly Mock<UserSetupEmailService> _userSetupService = TestUtil.CreateMock<UserSetupEmailService>();
        private readonly Mock<UserManager> _userManager = TestUtil.CreateMock<UserManager>();
        private readonly Mock<IDynComponentEmailer> _dynComponentEmailerMock = TestUtil.CreateMock<IDynComponentEmailer>();


        [TestInitialize]
        public void Init() {
            base.Init();
            _applicationMetadata = MetadataProvider.Application("person").StaticFromSchema("newPersonDetail");

            _udpdateMetadata = MetadataProvider.Application("person").StaticFromSchema("detail");

            TestUtil.ResetMocks(_swdbMock, _maximoEngine, _userSetupService, _userManager);

            var scanner = new TestSimpleInjectorScanner();
            scanner.ResgisterSingletonMock(_swdbMock);
            scanner.ResgisterSingletonMock(_maximoEngine);
            scanner.ResgisterSingletonMock(_userSetupService);
            scanner.ResgisterSingletonMock(_userManager);
            scanner.ResgisterSingletonMock(_dynComponentEmailerMock);

            scanner.InitDIController();
            var me = SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(typeof(MaximoConnectorEngine));
            var man = SimpleInjectorGenericFactory.Instance.GetObject<UserManager>(typeof(UserManager));

        }

        [TestMethod]
        public void TestCreation() {

            var json = JSonUtil.FromRelativePath("jsons\\person\\creation1.json");

            var ds = DataSetProvider.GetInstance();
            ds.HandleEvent(new ApplicationStartedEvent());
            var personDs = ds.LookupDataSet("person", null);

            Expression<Func<User, bool>> userComparison = u => !string.IsNullOrEmpty(u.Password) && u.IsActive.Value && u.UserName.Equals("personid") && u.MaximoPersonId.Equals("personid");

            var resultUser = new User();
            _userManager.Setup(x => x.SaveUser(It.Is(userComparison), false))
                .Callback<User, bool>((a, b) => resultUser = a)
                .Returns(() => resultUser);


            var resultObj = new TargetResult("150", "SWADMIN", null);

            _userSetupService.Setup(x => x.SendActivationEmail(It.Is(userComparison), "test@a.com", "test"));



            _maximoEngine.Setup(e => e.Execute(It.Is<OperationWrapper>(w => w.GetStringAttribute("personid").EqualsIc("personid") && w.OperationName.Equals(OperationConstants.CRUD_CREATE)))).Returns(() => resultObj);

            var result = personDs.Execute(_applicationMetadata, json, "-1", OperationConstants.CRUD_CREATE, false, null);


            TestUtil.VerifyMocks(_swdbMock, _maximoEngine, _userSetupService, _userManager);

        }


        [TestMethod]
        public void TestUpdateNoPassword() {

            var json = JSonUtil.FromRelativePath("jsons\\person\\update1.json");

            var ds = DataSetProvider.GetInstance();
            ds.HandleEvent(new ApplicationStartedEvent());
            var personDs = ds.LookupDataSet("person", null);

            Expression<Func<User, bool>> userComparison = u => string.IsNullOrEmpty(u.Password) && u.IsActive.Value && u.UserName.EqualsIc("SWADMIN") && u.MaximoPersonId.EqualsIc("SWADMIN");

            var resultUser = new User();
            _userManager.Setup(x => x.SaveUser(It.Is(userComparison), false))
                .Callback<User, bool>((a, b) => resultUser = a)
                .Returns(() => resultUser);

            var resultObj = new TargetResult("150", "SWADMIN", null);

            _maximoEngine.Setup(e =>e.Execute(It.Is<OperationWrapper>(w =>
                                w.GetStringAttribute("personid").EqualsIc("SWADMIN") &&
                                w.OperationName.Equals(OperationConstants.CRUD_UPDATE))))
                .Returns(() => resultObj);

            var result = personDs.Execute(_udpdateMetadata, json, "150", OperationConstants.CRUD_UPDATE, false, null);

            TestUtil.VerifyMocks(_swdbMock, _maximoEngine, _userSetupService, _userManager);

            var user = (User)result.ResultObject;

            Assert.AreEqual(true, user.IsActive);
            Assert.AreEqual("SWADMIN", user.UserName,true);
            Assert.AreEqual("SWADMIN", user.MaximoPersonId,true);
            

        }
    }
}