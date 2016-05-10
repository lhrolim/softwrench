using System.Linq;
using System.Reflection;
using cts.commons.simpleinjector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.test.Metadata;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Applications.Association;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Search.QuickSearch {
    [TestClass]
    public class LookupQuickSearchTest : BaseMetadataTest {
        private ApplicationAssociationResolver _resolver;

        [TestInitialize]
        public void Init() {
            if (ApplicationConfiguration.TestclientName != "test_only") {
                ApplicationConfiguration.TestclientName = "test_only";
                MetadataProvider.StubReset();
            }
            var scanner = new TestSimpleInjectorScanner();
            scanner.InitDIController();
            var injector = new SimpleInjectorGenericFactory(scanner.Container);
            _resolver = injector.GetObject<ApplicationAssociationResolver>(typeof (ApplicationAssociationResolver));
        }

        [TestMethod]
        public void TestWithTargetSelectStatement() {
            var expected = string.Format("(" +
                                         // primary attribute
                                         "(UPPER(COALESCE(fakefailurelistonly.failurecode,'')) like :{0})" + 
                                         "OR" +
                                         // complete select statement (precompiled --> variables already replaced)
                                         "(UPPER(COALESCE((select description from failurecode where failurecode = fakefailurelistonly.failurecode and orgid = fakefailurelistonly.orgid),'')) like :{0})" +
                                         ")", 
                                         QuickSearchHelper.QuickSearchParamName);
            var filter = new SearchRequestDto();

            var association = MetadataProvider.Application("fakeworkorder")
                .SchemasList.First(s => s.SchemaId == "TestWithTargetSelectStatement_detail")
                .Associations()
                .First(a => (EntityUtil.IsRelationshipNameEquals(a.AssociationKey, "failurelist_")));

            var method = _resolver.GetType().GetMethod("AppendQuickSearch", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(_resolver, new object[] { association, filter });

            Assert.IsFalse(string.IsNullOrEmpty(filter.WhereClause));
            Assert.IsTrue(filter.WhereClause.Contains(expected));
        }

    }
}
