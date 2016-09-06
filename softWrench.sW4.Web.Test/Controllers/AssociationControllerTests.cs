using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using cts.commons.simpleinjector;
using cts.commons.persistence;
using System.Collections.Generic;
using System.Dynamic;
using softWrench.sW4.Web.Controllers;
using softWrench.sW4.Data.Persistence.Dataset.Commons;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Security.Context;
using softWrench.sW4.Data.Filter;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.API.Association.Lookup;
using softwrench.sw4.Shared2.Data.Association;
using System.IO;
using softwrench.sW4.TestBase;

namespace softWrench.sW4.Web.Test.Controllers {
    [TestClass]
    public class AssociationControllerTests : BaseMetadataTest {

        /// <summary>
        /// Test method for the location lookup 
        /// Client metadata - Pesco
        /// </summary>
        [TestMethod]
        public void FetchLookupOptions_LocationLookupTest() {
            // Mock objects
            var swdbMock = new Mock<ISWDBHibernateDAO>();
            var maximodbMock = new Mock<IMaximoHibernateDAO>();
            var contextLookuperMock = new Mock<IContextLookuper>();
            var whereClauseFacadeMock = new Mock<FilterWhereClauseHandler>();
            var associationResolverMock = new Mock<ApplicationAssociationResolver>();
            var entityRepo = new EntityRepository(swdbMock.Object, maximodbMock.Object);

            // This is updated after the FindByNativeQuery callback.
            var queryParameter = new ExpandoObject();

            // set up mocks
            maximodbMock.Setup(x => x.FindByNativeQuery(It.IsAny<String>(), It.IsAny<ExpandoObject>(), It.IsAny<IPaginationData>(), It.IsAny<string>()))
                .Callback<string, ExpandoObject, IPaginationData, string>((a, b, c, d) => queryParameter = b)
                .Returns(() => {
                    var loc = new ExpandoObject() as IDictionary<string, Object>;
                    loc.Add("location", "location 1");
                    loc.Add("description", "location 1");
                    loc.Add("isproject", 1);
                    loc.Add("locationsid", 1234);
                    loc.Add("orgid", "PESCO 1");
                    loc.Add("siteid", "PVDISTRIBUTED 1");
                    loc.Add("type", "OPERATING 1");

                    return new List<dynamic>(){ loc };
                }
            );

            //Set up DI
            var scanner = new TestSimpleInjectorScanner();
            scanner.ResgisterSingletonMock<ISWDBHibernateDAO>(swdbMock);
            scanner.ResgisterSingletonMock<IMaximoHibernateDAO>(maximodbMock);
            scanner.ResgisterSingletonMock<IContextLookuper>(contextLookuperMock);
            scanner.ResgisterSingletonObject<EntityRepository>(entityRepo);
            scanner.InitDIController();

            var injector = new SimpleInjectorGenericFactory(scanner.Container);

            var dataSetProvider = DataSetProvider.GetInstance();
            dataSetProvider.Clear();


            // The test target
            ApplicationConfiguration.TestclientName = "pesco";
            MetadataProvider.StubReset();

            var target = new AssociationController(dataSetProvider, 
                new FilterWhereClauseHandler(new Data.Search.QuickSearch.QuickSearchHelper()), 
                contextLookuperMock.Object, new DataProviderResolver(new DynamicOptionFieldResolver(), new ApplicationAssociationResolver()));

            // Test data.
            var dto = new Data.API.Association.Lookup.LookupOptionsFetchRequestDTO() {
                ParentKey = new ApplicationMetadataSchemaKey("editdetail", SchemaMode.input, ClientPlatform.Web) { ApplicationName = "workorder" },
                AssociationFieldName = "location_",
                SearchDTO = new Data.Pagination.PaginatedSearchRequestDto(1, new List<int>() { 10, 20, 30 }) {
                    AddPreSelectedFilters = true,
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 30
                }
            };

            // Test
            var response = target.GetLookupOptions(dto, Newtonsoft.Json.Linq.JObject.Parse(new StreamReader("jsons\\workorder\\test5.json").ReadToEnd()));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.ResultObject is LookupOptionsFetchResultDTO);

            var count = 0;
            using (IEnumerator<IAssociationOption> enumerator = response.ResultObject.AssociationData.GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    IAssociationOption item = enumerator.Current;
                    count++;
                }
            }

            Assert.AreEqual(1, count);

            //Evaluate the queryParameter for Pesco metadata
            Assert.IsNotNull(queryParameter);
            Assert.AreEqual("1", (queryParameter as dynamic).isproject);
        }
    }
}