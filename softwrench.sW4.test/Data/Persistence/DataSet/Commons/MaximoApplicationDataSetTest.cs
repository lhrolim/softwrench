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
using softWrench.sW4.Data;
using softWrench.sW4.Data.API.Response;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;
using softWrench.sW4.Data.Persistence.Engine;
using softWrench.sW4.Data.Persistence.Operation;
using softWrench.sW4.Data.Persistence.WS.API;
using softWrench.sW4.Dynamic;
using softWrench.sW4.Dynamic.Services;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Persistence.DataSet.Commons {
    [TestClass]
    public class MaximoApplicationDataSetTest : BaseOtbMetadataTest {

        private ApplicationMetadata _applicationMetadata;


        private readonly Mock<IDynComponentEmailer> _dynComponentEmailerMock = TestUtil.CreateMock<IDynComponentEmailer>();
        private readonly Mock<MaximoConnectorEngine> _maximoEngine = TestUtil.CreateMock<MaximoConnectorEngine>();

        [TestInitialize]
        public override void Init() {
            base.Init();
            _applicationMetadata = MetadataProvider.Application("servicerequest").StaticFromSchema("newdetail");


            TestUtil.ResetMocks(_maximoEngine);

            var scanner = new TestSimpleInjectorScanner();
            scanner.ResgisterSingletonMock(_dynComponentEmailerMock);
            scanner.ResgisterSingletonMock(_maximoEngine);

            scanner.InitDIController();
        }


        [TestMethod]
        public async Task TestExecute() {

            var json = JSonUtil.FromRelativePath("jsons\\sr\\creation1.json");

            var ds = DataSetProvider.GetInstance();
            ds.HandleEvent(new ApplicationStartedEvent());
            var maximoDataSet = new FakeSRApplicationDataSet();

            var userSiteTuple = new Tuple<string, string>("100", "BEDFORD");
            var dm = DataMap.BlankInstance("servicerequest");
            _maximoEngine.Setup(e => e.FindById(It.Is<SlicedEntityMetadata>(w => w.ApplicationName.Equals("servicerequest")), "100", userSiteTuple)).ReturnsAsync(dm);

            var result = await maximoDataSet.Execute(_applicationMetadata, json, "-1", OperationConstants.CRUD_CREATE, false, null);

            TestUtil.VerifyMocks(_maximoEngine);
            Assert.AreEqual(result.ResultObject, dm);

        }


        [TestMethod]
        public async Task TestExecuteCreationReturningNullEntity() {

            var json = JSonUtil.FromRelativePath("jsons\\sr\\creation1.json");

            var ds = DataSetProvider.GetInstance();
            ds.HandleEvent(new ApplicationStartedEvent());
            var maximoDataSet = new FakeSRApplicationDataSet();

            var userSiteTuple = new Tuple<string, string>("100", "BEDFORD");
            var dm = DataMap.BlankInstance("servicerequest");
            _maximoEngine.Setup(e => e.FindById(It.Is<SlicedEntityMetadata>(w => w.ApplicationName.Equals("servicerequest")), "100", userSiteTuple)).ReturnsAsync(dm);

            var result = await maximoDataSet.Execute(_applicationMetadata, json, "-1", OperationConstants.CRUD_CREATE, false, null);

            TestUtil.VerifyMocks(_maximoEngine);
            Assert.AreEqual(result.ResultObject, dm);

        }

        public class FakeSRApplicationDataSet : BaseServiceRequestDataSet {
            public override TargetResult DoExecute(OperationWrapper operationWrapper) {
                var data = operationWrapper.OperationData() as CrudOperationData;
                data.ReloadMode = ReloadMode.MainDetail;
                return new TargetResult("100", "100", null);
            }

         

            public override string SchemaId() {
                return "newdetail";
            }
        }
    

    }
}
