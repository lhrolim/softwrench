using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Search;

namespace softwrench.sW4.test.Data.Search {
    [TestClass]
    public class SearchUtilsTest {

        [TestMethod]
        public void TestParamRegex() {
            var dto = new SearchRequestDto {
                SearchParams = "#application&&test",
                SearchValues = "123,,,333"
            };
            var map = SearchUtils.GetParameters(dto);
            Assert.IsTrue(map.ContainsKey("test"));
            Assert.IsTrue(map.ContainsKey("#application"));

            dto = new SearchRequestDto {
                SearchParams = "#application&&test||#asadfa",
                SearchValues = "123,,,333,,,4444"
            };
            map = SearchUtils.GetParameters(dto);
            Assert.IsTrue(map.ContainsKey("test"));
            Assert.IsTrue(map.ContainsKey("#application"));
            Assert.IsTrue(map.ContainsKey("#asadfa"));
        }

        [TestMethod]
        public void TestParamSingleTransient() {
            SearchRequestDto dto = new SearchRequestDto();
            dto.SearchParams = "#application";
            dto.SearchValues = "123";
            var map = SearchUtils.GetParameters(dto);
            Assert.IsTrue(map.ContainsKey("#application"));
        }
    }
}
