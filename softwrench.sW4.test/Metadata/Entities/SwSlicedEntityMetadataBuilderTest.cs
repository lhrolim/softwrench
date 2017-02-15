using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Util;
using softwrench.sW4.Shared2.Metadata.Applications.Schema.Interfaces;
using softwrench.sW4.TestBase;
using softWrench.sW4.Data.Pagination;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Data.Search;
using softWrench.sW4.Metadata.Entities;

namespace softwrench.sW4.test.Metadata.Entities {

    [TestClass]
    public class SwSlicedEntityMetadataBuilderTest : BaseMetadataTest {

        private static ApplicationSchemaDefinition _schema;
        private static ApplicationSchemaDefinition _wslistSchema;
        private static ApplicationSchemaDefinition _wsDetailSchema;

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            ApplicationConfiguration.TestclientName = "otb";
            MetadataProvider.StubReset();
            var schemas = MetadataProvider.Application("_SoftwrenchError").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("detail", "input", "web")];


            _wslistSchema =
                MetadataProvider.Application("_whereclause").Schemas()[
                    new ApplicationMetadataSchemaKey("list", "input", "web")];

            _wsDetailSchema =
               MetadataProvider.Application("_whereclause").Schemas()[
                   new ApplicationMetadataSchemaKey("detail", "input", "web")];
        }

        [TestMethod]
        public void TestSWDBSlicedEntity() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("Problem_"), _schema);
            IApplicationDisplayable rowstamp = new ApplicationFieldDefinition("_SoftwrenchError", "rowstamp", "rowstamp");
            Assert.IsFalse(sliced.AppSchema.Displayables.Contains(rowstamp));
            Assert.IsFalse(sliced.AppSchema.Displayables.Contains(null));
            Assert.AreNotEqual(sliced, null);
        }

        [TestMethod]
        public void TestSWDBSlicedEntityWithRelationships() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("whereclause_"), _wslistSchema);
            IApplicationDisplayable metadataId = new ApplicationFieldDefinition("_whereclause", "condition_.metadataid", "Metadata Id");
            Assert.IsTrue(sliced.AppSchema.Displayables.Contains(metadataId));
            Assert.IsFalse(sliced.AppSchema.Displayables.Contains(null));
            Assert.AreNotEqual(sliced, null);

            //enforcing loading of some internal structures of the SlicedEntityMetadata by this call
            var selectClause = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.List, new PaginatedSearchRequestDto());
            Debug.WriteLine(selectClause);
            Assert.IsTrue(selectClause.Contains("profile"));
            var fromClause = QueryFromBuilder.Build(sliced, new PaginatedSearchRequestDto());
            //            Debug.WriteLine(fromClause);
            Assert.IsTrue(fromClause.Contains("left join CONF_WCCONDITION"));
            Assert.IsTrue(fromClause.Contains("left join CONF_PROPERTYDEFINITION"));


            var dto = new PaginatedSearchRequestDto();
            dto.AppendProjectionField(ProjectionField.Default("userprofile"));
            dto.AppendProjectionField(ProjectionField.Default("condition_.metadataid"));
            dto.AppendProjectionField(ProjectionField.Default("#application"));
            dto.AppendProjectionField(ProjectionField.Default("propdefinition_.key"));
            fromClause = QueryFromBuilder.Build(sliced, dto);
            Debug.WriteLine(fromClause);
            Assert.IsTrue(fromClause.Contains("left join CONF_WCCONDITION"));
            Assert.IsTrue(fromClause.Contains("left join CONF_PROPERTYDEFINITION"));

        }

        [TestMethod]
        public void TestSWDBSlicedEntityForDetail() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("whereclause_"), _wsDetailSchema);
            IApplicationDisplayable metadataId = new ApplicationFieldDefinition("_whereclause", "condition_.metadataid", "Metadata Id");
//            Assert.IsTrue(sliced.AppSchema.Displayables.Contains(metadataId));
            Assert.IsFalse(sliced.AppSchema.Displayables.Contains(null));
            Assert.IsNull(sliced.Attributes(EntityMetadata.AttributesMode.NoCollections).FirstOrDefault(a=> a.Name.Equals("condition_.id")));
            Assert.AreNotEqual(sliced, null);

            //enforcing loading of some internal structures of the SlicedEntityMetadata by this call
            var selectClause = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.Detail, new PaginatedSearchRequestDto());
            Assert.IsFalse(selectClause.Contains("condition_.id"));
            Debug.WriteLine(selectClause);
            Assert.IsTrue(selectClause.Contains("profile"));
            var fromClause = QueryFromBuilder.Build(sliced, new PaginatedSearchRequestDto());
//                        Debug.WriteLine(fromClause);
            Assert.IsTrue(fromClause.Contains("left join CONF_WCCONDITION"));


            var dto = new PaginatedSearchRequestDto();
            dto.AppendProjectionField(ProjectionField.Default("userprofile"));
            dto.AppendProjectionField(ProjectionField.Default("condition_.metadataid"));
            dto.AppendProjectionField(ProjectionField.Default("#application"));
            dto.AppendProjectionField(ProjectionField.Default("propdefinition_.key"));
            fromClause = QueryFromBuilder.Build(sliced, dto);
            Debug.WriteLine(fromClause);
            Assert.IsTrue(fromClause.Contains("left join CONF_WCCONDITION"));

        }






    }
}
