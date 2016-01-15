using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.test.Util;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;
using System.Diagnostics;

namespace softwrench.sW4.test.Metadata {

    [TestClass]
    public class MenuParsingTest {


        [TestMethod]
        public void TestWebMenuParsing() {
            foreach (var clientName in TestUtil.ClientNames()) {
                if (clientName == "pae") {
                    //maximo 7.1
                    continue;
                }
                Debug.WriteLine(clientName);
                ApplicationConfiguration.TestclientName = clientName;
                MetadataProvider.StubReset();
                bool fromCache;
                var menu = InMemoryUser.TestInstance("test").Menu(ClientPlatform.Web, out fromCache);
                Assert.IsNotNull(menu);
            }

        }
    }
}
