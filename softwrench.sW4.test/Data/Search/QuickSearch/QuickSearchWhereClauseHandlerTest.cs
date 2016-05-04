using System;
using System.Collections.Generic;
using System.Linq;
using cts.commons.persistence;
using cts.commons.simpleinjector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.test.Metadata;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Stereotypes.Schema;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Search.QuickSearch {


    [TestClass]
    public class QuickSearchWhereClauseHandlerTest : BaseMetadataTest {

        private ApplicationSchemaDefinition _schema;
        private ApplicationSchemaDefinition _detailSchema;
        private Mock<QuickSearchHelper> _helperMock;
        private TestSimpleInjectorScanner _scanner;
        private QuickSearchWhereClauseHandler _handler;


        [TestInitialize]
        public void Init() {
            if (ApplicationConfiguration.TestclientName != "otb") {
                ApplicationConfiguration.TestclientName = "otb";
                MetadataProvider.StubReset();
            }
            var schemas = MetadataProvider.Application("servicerequest").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("list", "input", "web")];
            _detailSchema = schemas[new ApplicationMetadataSchemaKey("editdetail", "input", "web")];
            _helperMock = new Mock<QuickSearchHelper>();

            // not interested in testing this component here
            _helperMock.Setup(a => a.BuildOrWhereClause(It.IsAny<IEnumerable<string>>(), null)).Returns("1=1");

            _scanner = new TestSimpleInjectorScanner();
            _scanner.ResgisterSingletonMock(_helperMock);
            _scanner.InitDIController();
            var injector = new SimpleInjectorGenericFactory(_scanner.Container);
            _handler = injector.GetObject<QuickSearchWhereClauseHandler>(typeof(QuickSearchWhereClauseHandler));
        }

        /// <summary>
        /// Assures that if the schema has the property list.search.quicksearchfields defined, only its fields would be searched
        /// </summary>
        [TestMethod]
        public void TestWithCompositionDataAndSchemaProperty() {
            var dto = new PaginatedSearchRequestDto();

            dto.QuickSearchDTO = new QuickSearchDTO() {
                QuickSearchData = "test",
                CompositionsToInclude = new List<string>()
                {
                    "worklog"
                }
            };

            var composition = _detailSchema.Compositions().FirstOrDefault(a => a.Relationship == "worklog_");
            var worklogListSchema = composition.Schema.Schemas.List;
            worklogListSchema.Properties[ApplicationSchemaPropertiesCatalog.ListQuickSearchFields] = "description,createby";

            var result = _handler.HandleDTO(_schema, dto);

            worklogListSchema.Properties[ApplicationSchemaPropertiesCatalog.ListQuickSearchFields] = "description";

            _helperMock.Verify(a => a.BuildOrWhereClause(It.IsAny<IEnumerable<string>>(), null), Times.Once());

            Assert.AreEqual(
                "1=1 or exists (select 1 from worklog as worklog_ where SR.ticketid = worklog_.recordkey and SR.siteid = worklog_.siteid and worklog_.class = 'SR' and ((UPPER(COALESCE(worklog_.description,'')) like :quicksearchstring) or (UPPER(COALESCE(worklog_.createby,'')) like :quicksearchstring)))",
                result.WhereClause);




        }


        /// <summary>
        /// Assures that if the schema has the property list.search.quicksearchfields defined, only its fields would be searched
        /// </summary>
        [TestMethod]
        public void TestWithCompositionDataAndSchemaProperty2() {

            var dto = new PaginatedSearchRequestDto();
            dto.QuickSearchDTO = new QuickSearchDTO() {
                QuickSearchData = "test",
                CompositionsToInclude = new List<string>()
                {
                    "worklog","commlog"
                }
            };

            var whereClause = _handler.HandleDTO(_schema, dto).WhereClause;

            var expected =
@"1=1 
or exists (select 1 from worklog as worklog_ where SR.ticketid = worklog_.recordkey and SR.siteid = worklog_.siteid and worklog_.class = 'SR' and ((UPPER(COALESCE(worklog_.description,'')) like :quicksearchstring))) 
or exists (select 1 from commlog as commlog_ where SR.ticketuid = commlog_.ownerid and commlog_.ownertable = 'SR' and ((commlog_.sendto like :quicksearchstring) or (UPPER(COALESCE(commlog_.sendfrom,'')) like :quicksearchstring) or (UPPER(COALESCE(commlog_.subject,'')) like :quicksearchstring)))";
            Assert.AreEqual(expected.Replace("\n", "").Replace("\t", "").Replace("\r", ""), whereClause.Replace("\n", "").Replace("\t", "").Replace("\r", ""));
        }


//        /// <summary>
//        /// Assures that if the schema has the property list.search.quicksearchfields defined, only its fields would be searched
//        /// </summary>
//        [TestMethod]
//        public void TestWithCompositionDataAndSchemaProperty3() {
//
//            var dto = new PaginatedSearchRequestDto();
//            dto.QuickSearchDTO = new QuickSearchDTO() {
//                QuickSearchData = "test",
//                CompositionsToInclude = new List<string>(){
//                    "attachment"
//                }
//            };
//
//            var whereClause = _handler.HandleDTO(_schema, dto).WhereClause;
//
//            var expected =
//@"1=1 
//or exists (select 1 from worklog as worklog_ where SR.ticketid = worklog_.recordkey and SR.siteid = worklog_.siteid and worklog_.class = 'SR' and ((UPPER(COALESCE(worklog_.description,'')) like :quicksearchstring)))";
//            Assert.AreEqual(expected.Replace("\n", "").Replace("\t", "").Replace("\r", ""), whereClause.Replace("\n", "").Replace("\t", "").Replace("\r", ""));
//        }


        /// <summary>
        /// Assures that if the schema has the property list.search.quicksearchfields defined, only its fields would be searched
        /// </summary>
        [TestMethod]
        public void TestWithCompositionDataAndSchemaPropertyNoFieldsDeclared() {


            var dto = new PaginatedSearchRequestDto();

            dto.QuickSearchDTO = new QuickSearchDTO() {
                QuickSearchData = "test",
                CompositionsToInclude = new List<string>()
                {
                    "worklog"
                }
            };

            var composition = _detailSchema.Compositions().FirstOrDefault(a => a.Relationship == "worklog_");
            var worklogListSchema = composition.Schema.Schemas.List;
            worklogListSchema.Properties[ApplicationSchemaPropertiesCatalog.ListQuickSearchFields] = "";

            var result = _handler.HandleDTO(_schema, dto);

            worklogListSchema.Properties[ApplicationSchemaPropertiesCatalog.ListQuickSearchFields] = "description";

            _helperMock.Verify(a => a.BuildOrWhereClause(It.IsAny<IEnumerable<string>>(), null), Times.Once());

            //if list of fields is blank, do not add any clausues
            Assert.AreEqual("1=1", result.WhereClause);



        }
    }
}
