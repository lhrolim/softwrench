using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata.Applications.Association;

namespace softwrench.sW4.test.Metadata.Association {
    [TestClass]
    public class DependencyBuilderTest {
        
        [TestMethod]
        public void TestParseWhere() {
            var result = DependencyBuilder.TryParsingDependentFields("field1=#field1 and field2=#field2");
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("field1"));
            Assert.IsTrue(result.Contains("field2"));
        }

        [TestMethod]
        public void TestParseWhere2() {
            var result = DependencyBuilder.TryParsingDependentFields("location_id=#location");
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains("location"));
        }

        [TestMethod]
        public void TestParseWhereNull() {
            var result = DependencyBuilder.TryParsingDependentFields(null);
            Assert.AreEqual(0, result.Count);
        }
    }
}
