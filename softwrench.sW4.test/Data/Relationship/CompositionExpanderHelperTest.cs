using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Relationship.Composition;

namespace softwrench.sW4.test.Data.Relationship {

    [TestClass]
    public class CompositionExpanderHelperTest {

        [TestMethod]
        public void TestExpanding() {
            var result = CompositionExpanderHelper.ParseDictionary("worklog=100,200,300,,,attachments=200,400,500,,,test=lazy");
            Assert.AreEqual(2, result.DetailsToExpand.Count());
            Assert.AreEqual(1, result.ListsToExpand.Count());

            Assert.AreEqual(result.DetailsToExpand.FirstOrDefault().Key, "worklog");
            Assert.AreEqual(result.ListsToExpand.FirstOrDefault(),"test");

        }

        [TestMethod]
        public void TestExpandingNoId() {
            var result = CompositionExpanderHelper.ParseDictionary("worklog=,,,attachments=,,,test=lazy");
            Assert.AreEqual(0, result.DetailsToExpand.Count());
            Assert.AreEqual(1, result.ListsToExpand.Count());

            Assert.AreEqual(result.ListsToExpand.FirstOrDefault(), "test");

        }
    }
}
