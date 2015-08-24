using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Configuration.Util;

namespace softwrench.sW4.test.Category {
    [TestClass]
    public class CategoryUtilTest {
        
        [TestMethod]
        public void Simple()
        {
            var result =CategoryUtil.BuildCategoryEntries("/a/");
            var keyValuePairs = result as KeyValuePair<string, softWrench.sW4.Configuration.Definitions.Category>[] ?? result.ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(1,keyValuePairs.Count());
            var entry = keyValuePairs.FirstOrDefault();
            Assert.AreEqual("/a/", entry.Key);
            Assert.AreEqual(new softWrench.sW4.Configuration.Definitions.Category{Key = "a",ParentCategory = null}, entry.Value);
        }

        [TestMethod]
        public void TwoLevels() {
            var result = CategoryUtil.BuildCategoryEntries("/a/b/");
            var keyValuePairs = result as KeyValuePair<string, softWrench.sW4.Configuration.Definitions.Category>[] ?? result.ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(2, keyValuePairs.Count());
            var entry = keyValuePairs.FirstOrDefault();
            var first =keyValuePairs[0];
            var second =keyValuePairs[1];
            var parentCategory = new softWrench.sW4.Configuration.Definitions.Category { Key = "a", ParentCategory = null };
            Assert.AreEqual("/a/b/", first.Key);
            Assert.AreEqual(new softWrench.sW4.Configuration.Definitions.Category { Key = "b", ParentCategory = parentCategory }, first.Value);
            Assert.AreEqual("/a/", second.Key);
            Assert.AreEqual(parentCategory, second.Value);

        }

        [TestMethod]
        public void ThreeLevels() {
            var result = CategoryUtil.BuildCategoryEntries("a/b/c");
            var keyValuePairs = result as KeyValuePair<string, softWrench.sW4.Configuration.Definitions.Category>[] ?? result.ToArray();
            Assert.IsNotNull(result);
            Assert.AreEqual(3, keyValuePairs.Count());
            var entry = keyValuePairs.FirstOrDefault();
            var first = keyValuePairs[0];
            var second = keyValuePairs[1];
            var third = keyValuePairs[2];
            var grandParentCategory = new softWrench.sW4.Configuration.Definitions.Category { Key = "a", ParentCategory = null };
            var parentCategory = new softWrench.sW4.Configuration.Definitions.Category { Key = "b", ParentCategory = grandParentCategory };
          
            Assert.AreEqual("/a/b/c/", first.Key);
            Assert.AreEqual(new softWrench.sW4.Configuration.Definitions.Category { Key = "c", ParentCategory = parentCategory }, first.Value);
           
            Assert.AreEqual("/a/b/", second.Key);
            Assert.AreEqual(parentCategory, second.Value);

            Assert.AreEqual("/a/", third.Key);
            Assert.AreEqual(grandParentCategory, third.Value);

        }

        [TestMethod]
        public void PropertyKey()
        {
            Assert.AreEqual("prop1",CategoryUtil.GetPropertyKey("/a/b/prop1"));
        }

    }
}
