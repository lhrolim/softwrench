using System;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Persistence.Relational.EntityRepository;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities;

namespace softwrench.sW4.test.Data.Relationship {
    /// <summary>
    /// Summary description for RestEntityRepositoryTest
    /// </summary>
    [TestClass]
    public class RestEntityRepositoryTest : BaseOtbMetadataTest {

        private static EntityMetadata _entity;
        private static readonly RestEntityRepository RestClient = new RestEntityRepository(null);


        [TestInitialize]
        public override void Init() {
            base.Init();
            _entity = MetadataProvider.Entity("sr");
        }

        [TestMethod]
        public void TestQueryById() {
            var result = RestClient.BuildGetUrl(_entity, new SearchRequestDto() {Id = "10"}, "ism");
            Assert.AreEqual("http://localhost:8080/maxrest/rest/mbo/SR/10?_urs=true&pluspcustomer=~eq~TTC-00", result);
        }


        [TestMethod]
        public void TestQueryWithIn() {
            var dto = new SearchRequestDto();
            var projectionFields = dto.ProjectionFields;
            projectionFields.Add(ProjectionField.Default("status"));
            projectionFields.Add(ProjectionField.Default("ticketuid"));
            projectionFields.Add(ProjectionField.Default("ticketid"));
            dto.AppendSearchEntry("ticketuid", "2,3,4");
            dto.AppendSearchEntry("status", "APPR");
            

            var result = RestClient.BuildGetUrl(_entity, dto, "ism");
            Assert.IsTrue(result.StartsWith("http://localhost:8080/maxrest/rest/mbo/SR/?"));

            var uri = new Uri(result);
            var query = HttpUtility.ParseQueryString(uri.Query);

            var colsToInclude = query.Get("_includecols");

            Assert.AreEqual("status,ticketid,ticketuid",colsToInclude);
            Assert.AreEqual("fake",query.Get("ticketuid.ormode"));
            Assert.AreEqual("2,3,4",query.Get("ticketuid"));
            Assert.AreEqual("~eq~APPR", query.Get("status"));
            Assert.AreEqual("~eq~TTC-00", query.Get("pluspcustomer"));
            Assert.AreEqual("true", query.Get("_urs"));

            var maxitems = query.Get("_maxitems");
            Assert.AreEqual(maxitems, "100");

            Assert.AreEqual(query.Count,7);


        }


        [TestMethod]
        public void TestQueryWithGt() {
            var dto = new SearchRequestDto();
            var projectionFields = dto.ProjectionFields;
            projectionFields.Add(ProjectionField.Default("status"));
            projectionFields.Add(ProjectionField.Default("ticketuid"));
            projectionFields.Add(ProjectionField.Default("ticketid"));
            dto.AppendSearchEntry("changedate", ">2016-03-10 05:41:07.107");


            var result = RestClient.BuildGetUrl(_entity, dto, "ism");
            Assert.IsTrue(result.StartsWith("http://localhost:8080/maxrest/rest/mbo/SR/?"));

            var uri = new Uri(result);
            var query = HttpUtility.ParseQueryString(uri.Query);

            Assert.AreEqual("status,ticketid,ticketuid",query.Get("_includecols"));
            Assert.AreEqual("~gt~2016-03-10T05:41:07.107", query.Get("changedate"));
            Assert.AreEqual("true",query.Get("_urs"));
            Assert.AreEqual("~eq~TTC-00", query.Get("pluspcustomer"));

            Assert.AreEqual("100",query.Get("_maxitems"));

            Assert.AreEqual(query.Count, 5);


        }

        [TestMethod]
        public void TestQueryWithEqualAndLike() {
            var dto = new SearchRequestDto();
            var projectionFields = dto.ProjectionFields;
            projectionFields.Add(ProjectionField.Default("status"));
            projectionFields.Add(ProjectionField.Default("ticketuid"));
            projectionFields.Add(ProjectionField.Default("ticketid"));
            dto.AppendSearchEntry("description", "%test%");
            dto.AppendSearchEntry("summary", "test%");
            dto.AppendSearchEntry("status", "APPR");


            var result = RestClient.BuildGetUrl(_entity, dto, "ism");
            Assert.IsTrue(result.StartsWith("http://localhost:8080/maxrest/rest/mbo/SR/?"));

            var uri = new Uri(result);
            var query = HttpUtility.ParseQueryString(uri.Query);

            var colsToInclude = query.Get("_includecols");
            Assert.AreEqual(colsToInclude, "status,ticketid,ticketuid");


            Assert.AreEqual("~sw~TEST", query.Get("summary"));

            Assert.AreEqual("~eq~APPR", query.Get("status"));

            Assert.AreEqual("TEST", query.Get("description"));

            Assert.AreEqual("true", query.Get("_urs"));

            Assert.AreEqual("100",query.Get("_maxitems"));
            Assert.AreEqual("~eq~TTC-00", query.Get("pluspcustomer"));

            Assert.AreEqual(query.Count, 7);


        }


        /// <summary>
        /// Not testing other scenarios, just the ordering
        /// </summary>
        [TestMethod]
        public void TestQueryWithOrderBy() {

            var ascendingDTO = new SearchRequestDto {SearchSort = "statusdate"};
            var result = RestClient.BuildGetUrl(_entity, ascendingDTO, "ism");
            var query = HttpUtility.ParseQueryString(new Uri(result).Query);
            Assert.AreEqual("statusdate", query.Get("_orderbydesc"));

            var descendingDTO = new SearchRequestDto { SearchSort = "statusdate", SearchAscending = true};
            result = RestClient.BuildGetUrl(_entity, descendingDTO, "ism");
            query = HttpUtility.ParseQueryString(new Uri(result).Query);
            Assert.AreEqual("statusdate", query.Get("_orderbyasc"));
        }

        public override string GetClientName() {
            return "testrest";
        }
    }
}
