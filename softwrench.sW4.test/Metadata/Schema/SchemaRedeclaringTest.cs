using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Metadata;

namespace softwrench.sW4.test.Metadata.Schema {
    [TestClass]
    public class SchemaRedeclaringTest : BaseOtbMetadataTest{

        [TestMethod]
        public void TestMatUseTransRedeclare()
        {
            var detailSchema = MetadataProvider.Application("matusetrans").Schema(new ApplicationMetadataSchemaKey("detail"));
            Assert.IsNull((detailSchema.Associations().FirstOrDefault(a => a.Target == "gldebitacct")));
        }


        public override string GetClientName() {
            return "firstsolar";
        }
    }
}
