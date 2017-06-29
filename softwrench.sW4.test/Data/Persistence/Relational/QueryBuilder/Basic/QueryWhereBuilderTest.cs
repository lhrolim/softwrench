using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using cts.commons.persistence;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Metadata.Security;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Persistence.Relational.QueryBuilder.Basic
{
    [TestClass]
    public class QueryWhereBuilderTest
    {
        readonly IEnumerable<string> attributes = new List<string> { "itemnum", "description" };
        readonly IEnumerable<string> attributesWithRelationships = new List<string> { "itemnum", "location_.location", "location_.description", "description" };

        private readonly QuickSearchHelper QuickSearchHelper = new QuickSearchHelper();

        [TestInitialize]
        public void Init() { }

        [TestMethod]
        public void TestOrWhereClauseWithoutRelationshipAttributes()
        {
            var result = QuickSearchHelper.BuildOrWhereClause(DBType.Swdb, attributes, "item");
            Assert.IsTrue(result.Contains("((UPPER(COALESCE(item.itemnum,'')) like :quicksearchstring)OR(UPPER(COALESCE(item.description,'')) like :quicksearchstring))"));
        }

        [TestMethod]
        public void TestOrWhereClauseWithRelationshipAttributes()
        {
            var result = QuickSearchHelper.BuildOrWhereClause(DBType.Swdb,attributesWithRelationships, "item");
            Assert.IsTrue(result.Contains("((UPPER(COALESCE(item.itemnum,'')) like :quicksearchstring)OR(UPPER(COALESCE(item.description,'')) like :quicksearchstring)OR(UPPER(COALESCE(location_.location,'')) like :quicksearchstring)OR(UPPER(COALESCE(location_.description,'')) like :quicksearchstring))"));
        }

        public void TestOrWhereClauseWithoutContext()
        {
            var result = QuickSearchHelper.BuildOrWhereClause(DBType.Swdb,attributes);
            Assert.IsTrue(result.Contains("((UPPER(COALESCE(itemnum,'')) like :quicksearchstring)OR(UPPER(COALESCE(description,'')) like :quicksearchstring))"));
        }
    }
}
