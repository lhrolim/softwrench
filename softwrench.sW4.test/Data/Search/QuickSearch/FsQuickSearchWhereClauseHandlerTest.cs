using System;
using System.Collections.Generic;
using System.Diagnostics;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.TestBase;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Search.QuickSearch {
    [TestClass]
    public class FsQuickSearchWhereClauseHandlerTest : BaseOtbMetadataTest {
        private ApplicationSchemaDefinition _schema;
        private Mock<QuickSearchHelper> _helperMock;
        private TestSimpleInjectorScanner _scanner;
        private QuickSearchWhereClauseHandler _handler;

        [TestInitialize]
        public override void Init()
        {
            base.Init();
            var schemas = MetadataProvider.Application("_workpackage").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("listdeprecatedfortest", "input", "web")];

            _helperMock = new Mock<QuickSearchHelper>();
            // not interested in testing this component here
            _helperMock.Setup(a => a.BuildOrWhereClause(DBType.Maximo, It.IsAny<IEnumerable<string>>(), null,null)).Returns("1=1");

            _scanner = new TestSimpleInjectorScanner();
            _scanner.ResgisterSingletonMock(_helperMock);
            _scanner.InitDIController();
            var injector = new SimpleInjectorGenericFactory(_scanner.Container);
            _handler = injector.GetObject<QuickSearchWhereClauseHandler>(typeof(QuickSearchWhereClauseHandler));
        }


        [TestMethod]
        public void TestQSExclusion() {
            var dto = new PaginatedSearchRequestDto();
            dto.QuickSearchDTO = new QuickSearchDTO {
                QuickSearchData = "test",
                HiddenFieldsToInclude = new List<string>() { "wonum" }
            };
            var paginatedSearchRequestDto = _handler.HandleDTO(_schema, dto);
            Debug.WriteLine(paginatedSearchRequestDto.WhereClause);
        }

        public override string GetClientName() {
            return "firstsolar";
        }
    }
}
