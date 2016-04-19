using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using softWrench.sW4.Metadata;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Util {
    [TestClass]
    public class I18NResolverTest : BaseMetadataTest {

        private readonly I18NResolver _resolver = new I18NResolver();

        [TestMethod]
        public void LocateFiles() {
            ApplicationConfiguration.TestclientName = "hapag";
            var catalogs = _resolver.FetchCatalogs();
            Assert.AreEqual(3, catalogs.Count);

            var pritModalObj = ((JObject)catalogs["en"])["printmodal"];
            Assert.IsNotNull(pritModalObj);
            Assert.AreEqual(pritModalObj["print"].ToString(), "Print");

            pritModalObj = ((JObject)catalogs["es"])["printmodal"];
            Assert.IsNotNull(pritModalObj);
            Assert.AreEqual(pritModalObj["print"].ToString(), "Imprimirse");

            pritModalObj = ((JObject)catalogs["de"])["printmodal"];
            Assert.IsNotNull(pritModalObj);
//            Assert.AreEqual(pritModalObj["print"].ToString(), "Drucken");
        }

        /// <summary>
        /// Just ensures that all catalogs got parsed successfully
        /// </summary>
        [TestMethod]
        public void TestAllCatalogsOk() {
            foreach (var clientName in TestUtil.ClientNames()) {
                ApplicationConfiguration.TestclientName = clientName;
                var catalogs = _resolver.FetchCatalogs();
            }
        }

        [TestMethod]
        public void TestSchemaTitle() {
            ApplicationConfiguration.TestclientName = "hapag";
            MetadataProvider.StubReset();
            var catalogs = _resolver.FetchCatalogs();
            var schemas = MetadataProvider.Application("servicerequest").Schemas();
            var schema = schemas[new ApplicationMetadataSchemaKey("list", "input", "web")];
            var value = _resolver.DoGetI18NSchemaTitle(schema, "de");
            Assert.AreEqual("Service Request Tabelle", value);

            var missingSchema = new ApplicationSchemaDefinition {
                SchemaId = "fake",
                ApplicationName = "fake",
                Title = "fake"
            };
            value = _resolver.DoGetI18NSchemaTitle(missingSchema, "de");
            Assert.AreEqual("fake", value);


            missingSchema = new ApplicationSchemaDefinition {
                ApplicationName = "servicerequest",
                SchemaId = "fake",
                Title = "fake"
            };
            value = _resolver.DoGetI18NSchemaTitle(missingSchema, "de");
            Assert.AreEqual("fake", value);

            ApplicationConfiguration.TestclientName = "fake";
            _resolver.ClearCache();
            value = _resolver.DoGetI18NSchemaTitle(schema, "de");
            Assert.AreEqual(schema.Title,value);
        }
    }
}
