using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using cts.commons.persistence;
using cts.commons.portable.Util;
using cts.commons.simpleinjector;
using cts.commons.simpleinjector.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.user.classes.entities;
using softwrench.sw4.user.classes.services;
using softwrench.sw4.user.classes.services.setup;
using softwrench.sW4.test.Util;
using softwrench.sW4.TestBase;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Dynamic;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
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
        private readonly Mock<UserPasswordService> _passwordHistoryService = TestUtil.CreateMock<UserPasswordService>();


        [TestInitialize]
        public void Init() {
            base.Init();
            _applicationMetadata = MetadataProvider.Application("person").StaticFromSchema("newPersonDetail");

            _udpdateMetadata = MetadataProvider.Application("person").StaticFromSchema("detail");

            TestUtil.ResetMocks(_swdbMock, _maximoEngine, _userSetupService, _userManager, _passwordHistoryService);

            var scanner = new TestSimpleInjectorScanner();
            scanner.ResgisterSingletonMock(_swdbMock);
            scanner.ResgisterSingletonMock(_maximoEngine);
            scanner.ResgisterSingletonMock(_userSetupService);
            scanner.ResgisterSingletonMock(_userManager);
            scanner.ResgisterSingletonMock(_dynComponentEmailerMock);
            scanner.ResgisterSingletonMock(_passwordHistoryService);

            scanner.InitDIController();
            var me = SimpleInjectorGenericFactory.Instance.GetObject<MaximoConnectorEngine>(typeof(MaximoConnectorEngine));
            var man = SimpleInjectorGenericFactory.Instance.GetObject<UserManager>(typeof(UserManager));

        }

        [TestMethod]
        public async Task TestCreation() {

            var json = JSonUtil.FromRelativePath("jsons\\person\\creation1.json");

            var ds = DataSetProvider.GetInstance();
            ds.HandleEvent(new ApplicationStartedEvent());
            var personDs = ds.LookupDataSet("person", null);

            Expression<Func<User, bool>> userComparison = u => !string.IsNullOrEmpty(u.Password) && u.IsActive.Value && u.UserName.Equals("personid") && u.MaximoPersonId.Equals("personid");

            var resultUser = new User {
                UserName = "personid",
                Email = "test@a.com",
                Password = "any",
                MaximoPersonId = "personid",
                IsActive = true
            };
            _userManager.Setup(x => x.SaveUser(It.Is(userComparison), false))
                .Callback<User, bool>((a, b) => resultUser = a)
                .ReturnsAsync(resultUser);


            var resultObj = new TargetResult("150", "SWADMIN", null);

            _userSetupService.Setup(x => x.SendActivationEmail(It.Is(userComparison), "test@a.com", "test"));

            _passwordHistoryService.Setup(s => s.HandlePasswordHistory(It.IsAny<User>(), It.IsAny<string>())).Returns(Task.CompletedTask);


            _maximoEngine.Setup(e => e.Execute(It.Is<OperationWrapper>(w => w.GetStringAttribute("personid").EqualsIc("personid") && w.OperationName.Equals(OperationConstants.CRUD_CREATE)))).Returns(() => resultObj);

            var result = await personDs.Execute(_applicationMetadata, json, "-1", OperationConstants.CRUD_CREATE, false, null);


            TestUtil.VerifyMocks(_swdbMock, _maximoEngine, _userSetupService, _userManager, _passwordHistoryService);

        }


        [TestMethod]
        public async Task TestUpdateNoPassword() {

            var json = JSonUtil.FromRelativePath("jsons\\person\\update1.json");

            var ds = DataSetProvider.GetInstance();
            ds.HandleEvent(new ApplicationStartedEvent());
            var personDs = ds.LookupDataSet("person", null);

            Expression<Func<User, bool>> userComparison = u => string.IsNullOrEmpty(u.Password) && u.IsActive.Value && u.UserName.EqualsIc("SWADMIN") && u.MaximoPersonId.EqualsIc("SWADMIN");

            var resultUser = new User {
                UserName = "SWADMIN",
                IsActive = true,
                MaximoPersonId = "SWADMIN"
            };
            _userManager.Setup(x => x.SaveUser(It.Is(userComparison), false))
                .Callback<User, bool>((a, b) => resultUser = a)
                .ReturnsAsync(resultUser);

            var resultObj = new TargetResult("150", "SWADMIN", null);

            _maximoEngine.Setup(e => e.Execute(It.Is<OperationWrapper>(w =>
                                 w.GetStringAttribute("personid").EqualsIc("SWADMIN") &&
                                 w.OperationName.Equals(OperationConstants.CRUD_UPDATE))))
                .Returns(() => resultObj);

            var result = await personDs.Execute(_udpdateMetadata, json, "150", OperationConstants.CRUD_UPDATE, false, null);

            TestUtil.VerifyMocks(_swdbMock, _maximoEngine, _userSetupService, _userManager);

            var user = (User)result.ResultObject;

            Assert.AreEqual(true, user.IsActive);
            Assert.AreEqual("SWADMIN", user.UserName, true);
            Assert.AreEqual("SWADMIN", user.MaximoPersonId, true);


        }
    }
}
