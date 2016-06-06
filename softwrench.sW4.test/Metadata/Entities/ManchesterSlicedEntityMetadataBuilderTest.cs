using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.TestBase;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder;
using softWrench.sW4.Data.Persistence.Relational.QueryBuilder.Basic;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Entities.Sliced;
using softWrench.sW4.Util;
using System.Diagnostics;

namespace softwrench.sW4.test.Metadata.Entities
{
    [TestClass]
    public class ManchesterSlicedEntityMetadataBuilderTest : BaseMetadataTest {
        private static ApplicationSchemaDefinition _schema;

        [TestInitialize]
        public void Init()
        {
            ApplicationConfiguration.TestclientName = "manchester";
            MetadataProvider.StubReset();
            var schemas = MetadataProvider.Application("servicerequest").Schemas();
            _schema = schemas[new ApplicationMetadataSchemaKey("editdetail", null, "web")];
        }

        [TestMethod]
        public void TestReverseMapping() {
            var sliced = SlicedEntityMetadataBuilder.GetInstance(MetadataProvider.Entity("sr"), _schema, 300, true);
            var result = QuerySelectBuilder.BuildSelectAttributesClause(sliced, QueryCacheKey.QueryMode.Detail);
            Debug.Write(result);

            //should not cointain the reverse associations here
            Assert.IsFalse(result.Contains("tkserviceaddress"));
            var from = QueryFromBuilder.Build(sliced);
            Debug.Write(from);
            Assert.IsFalse(from.Contains("tkserviceaddress"));
        }
    }
}
