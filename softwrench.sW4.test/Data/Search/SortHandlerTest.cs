using System;
using System.Text;
using System.Collections.Generic;
using cts.commons.simpleinjector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.TestBase;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Data.Search.QuickSearch;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Data.Search {

    [TestClass]
    public class SortHandlerTest : BaseOtbMetadataTest {


        private ApplicationSchemaDefinition _wcListSchema;

        [TestInitialize]
        public override void Init() {
            base.Init();
            _wcListSchema =
                MetadataProvider.Application("_whereclause").Schemas()[new ApplicationMetadataSchemaKey("list")];
        }

        [TestMethod]
        public void ModifyAttributeToServerWC() {
            var handler = new SortHandler();
            var dto = new SearchRequestDto {
                SearchSort = "#application desc"
            };
            handler.HandleSearchDTO(_wcListSchema, dto);
            Assert.AreEqual("definition_id desc", dto.TranslatedSearchSort);
        }

        [TestMethod]
        public void ModifyAttributeToServerMultiSort() {
            var handler = new SortHandler();
            var dto = new SearchRequestDto {
                MultiSearchSort = new List<SortOrder>
                {
                    new SortOrder {ColumnName = "#application"}
                }
            };
            handler.HandleSearchDTO(_wcListSchema, dto);
            Assert.IsTrue(dto.TranslatedMultiSearchSort.Contains(new SortOrder {ColumnName = "definition_id"}));
        }

    }
}
