using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Configuration.Definitions;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Configuration.Util;

namespace softwrench.sW4.test.Category {
    [TestClass]
    public class CategoryTreeCacheTest {



        [TestMethod]
        public void TestGlobal() {
            IEnumerable<PropertyDefinition> list = TestList();

            var result = CategoryTreeCache.BuildCache(list,ConfigTypes.Global);

            Assert.AreEqual(1, result.Count());

            var enumerator = result.GetEnumerator();
            enumerator.MoveNext();

            var first = enumerator.Current;
            result = first.Children;
            

            //two root nodes --> /a/ and /d/
            Assert.AreEqual(2, result.Count());

            enumerator = result.GetEnumerator();
            enumerator.MoveNext();

            first = enumerator.Current;
            Assert.AreEqual("/Global/a/", first.FullKey);
            // /a/b and /a/c/
            Assert.AreEqual(2, first.Children.Count());
            // /a/prop5 and /a/prop3
            Assert.AreEqual(2, first.Definitions.Count());

            enumerator.MoveNext();
            CategoryDTO second = enumerator.Current;
            Assert.AreEqual("/Global/d/", second.FullKey);
            Assert.AreEqual(1, second.Definitions.Count());
            Assert.AreEqual("/Global/d/prop4", second.Definitions.First().FullKey);
        }

        public void TestWC() {

            var list = TestList();

            var result = CategoryTreeCache.BuildCache(list, ConfigTypes.WhereClauses);

            //just _whereclauses
            Assert.AreEqual(1, result.Count());

            var enumerator = result.GetEnumerator();
            enumerator.MoveNext();

            var first = enumerator.Current;
            result = first.Children;


            //two root nodes --> /a/ and /b/ and /c/
            Assert.AreEqual(3, result.Count());

            enumerator = result.GetEnumerator();
            enumerator.MoveNext();

            first = enumerator.Current;
            Assert.AreEqual("/_whereclauses/a/", first.FullKey);
            // /a/prop5 and /a/prop3
            Assert.AreEqual(1, first.Definitions.Count());

            enumerator.MoveNext();
            CategoryDTO second = enumerator.Current;
            Assert.AreEqual("/_whereclauses/b/", second.FullKey);
            Assert.AreEqual(1, second.Definitions.Count());
            Assert.AreEqual("/_whereclauses/b/whereclause", second.Definitions.First().FullKey);
        }


        [TestMethod]
        public void TestUpdate() {
            var result = CategoryTreeCache.BuildCache(TestList(), ConfigTypes.Global);
            Assert.IsTrue(CategoryTreeCache.DoUpdate(new CategoryDTO("/Global/a/b/"), result));
        }

        private static IEnumerable<PropertyDefinition> TestList() {
            IEnumerable<PropertyDefinition> list = new List<PropertyDefinition>()
            {
                new PropertyDefinition("/Global/a/prop5"),
                new PropertyDefinition("/Global/a/b/prop1"),
                new PropertyDefinition("/Global/a/b/prop2"),
                new PropertyDefinition("/Global/a/prop3"),
                new PropertyDefinition("/Global/a/c/prop3"),
                new PropertyDefinition("/Global/d/prop4"),
                new PropertyDefinition("/_whereclauses/a/whereclause"),
                new PropertyDefinition("/_whereclauses/b/whereclause"),
                new PropertyDefinition("/_whereclauses/c/whereclause"),
            };
            return list;
        }
    }
}
