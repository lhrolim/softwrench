using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        [TestMethod]
        public void GIVEN_AttributeTransient_AND_SearchValueStartsWithString_THEN_ValidQuery() {
            SearchRequestDto dto = new SearchRequestDto();
            dto.SearchParams = "#field";
            dto.SearchValues = "prefix%";
            var map = SearchUtils.GetParameters(dto);
            Assert.IsTrue(map.Values.Contains("prefix%".ToUpper()));
        }

        [TestMethod]
        public void GIVEN_AttributeTransient_AND_SearchValueEndsWithString_THEN_ValidQuery() {
            SearchRequestDto dto = new SearchRequestDto();
            dto.SearchParams = "#field";
            dto.SearchValues = "%sufix";
            var map = SearchUtils.GetParameters(dto);
            Assert.IsTrue(map.Values.Contains("%sufix".ToUpper()));
        }

        [TestMethod]
        public void GIVEN_AttributeTransient_AND_SearchValueContains_StartsWithString_EndsWithString_THEN_ValidQuery() {
            SearchRequestDto dto = new SearchRequestDto();
            dto.SearchParams = "#field";
            dto.SearchValues = "=prefix%,%sufix";

            var mapUtilsParameters = SearchUtils.GetParameters(dto);
            var utilsParameters = mapUtilsParameters.First().Value as IList<string>;

            var mapRequestParameters = dto.GetParameters().Values.ToList()[0];
            var requestParameters = mapRequestParameters != null ? mapRequestParameters.Value as IList<string> : null;

            Assert.IsTrue(utilsParameters != null);
            Assert.IsTrue(utilsParameters.Count == 2);
            Assert.IsTrue(utilsParameters.Contains("prefix%".ToUpper()));
            Assert.IsTrue(utilsParameters.Contains("%sufix".ToUpper()));

            Assert.IsTrue(requestParameters != null);
            Assert.IsTrue(requestParameters.Count == 2);
            Assert.IsTrue(mapRequestParameters.SearchOperator.Equals(SearchOperator.ORCONTAINS));
            Assert.IsTrue(requestParameters.Contains("prefix%".ToUpper()));
            Assert.IsTrue(requestParameters.Contains("%sufix".ToUpper()));
        }

        [TestMethod]
        public void GIVEN_AttributeTransient_AND_SearchValueContains_StartsWithString_EndsWithString_OrIsNull_THEN_ValidQuery() {
            SearchRequestDto dto = new SearchRequestDto();
            dto.SearchParams = "#field";
            dto.SearchValues = "=prefix%,%sufix,NULL";

            var mapUtilsParameters = SearchUtils.GetParameters(dto);
            var utilsParameters = mapUtilsParameters.First().Value as IList<string>;

            var mapRequestParameters = dto.GetParameters().Values.ToList()[0];
            var requestParameters = mapRequestParameters != null ? mapRequestParameters.Value as IList<string> : null;

            Assert.IsTrue(utilsParameters != null);
            Assert.IsTrue(utilsParameters.Count == 2);
            Assert.IsTrue(utilsParameters.Contains("prefix%".ToUpper()));
            Assert.IsTrue(utilsParameters.Contains("%sufix".ToUpper()));

            Assert.IsTrue(requestParameters != null);
            Assert.IsTrue(requestParameters.Count == 2);
            Assert.IsTrue(mapRequestParameters.NullOr);
            Assert.IsTrue(mapRequestParameters.SearchOperator.Equals(SearchOperator.ORCONTAINS));
            Assert.IsTrue(requestParameters.Contains("prefix%".ToUpper()));
            Assert.IsTrue(requestParameters.Contains("%sufix".ToUpper()));
        }

        [TestMethod]
        public void GIVEN_AttributeTransient_AND_SearchValueIsNull_THEN_ValidQuery() {
            SearchRequestDto dto = new SearchRequestDto();
            dto.SearchParams = "#field";
            dto.SearchValues = "=NULL";

            var mapUtilsParameters = SearchUtils.GetParameters(dto);
            var utilsParameters = mapUtilsParameters.First().Value.ToString();

            var mapRequestParameters = dto.GetParameters().Values.ToList()[0];
            var requestParameters = mapRequestParameters.Value.ToString();

            Assert.IsTrue(utilsParameters != null);
            Assert.IsTrue(utilsParameters.Contains("NULL"));

            Assert.IsTrue(requestParameters != null);
            Assert.IsTrue(requestParameters.Contains("NULL"));
            Assert.IsTrue(mapRequestParameters.IsNullOnly);
            Assert.IsTrue(mapRequestParameters.SearchOperator.Equals(SearchOperator.NULLONLY));
        }
    }
}
