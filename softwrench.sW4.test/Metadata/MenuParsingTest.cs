using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sW4.Shared2.Metadata.Applications;
using softwrench.sW4.test.Util;
using softWrench.sW4.Metadata;
using softWrench.sW4.Metadata.Security;
using softWrench.sW4.Util;
using System.Diagnostics;
using cts.commons.portable.Util;
using softWrench.sW4.Metadata.Menu;

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
                try {
                    MetadataProvider.StubReset();
                } catch (Exception e) {
                    throw new Exception("client {0} failed".Fmt(clientName),e);
                }

                bool fromCache;
                var user = InMemoryUser.TestInstance("test");
                var menu = new MenuSecurityManager().Menu(user, ClientPlatform.Web, out fromCache);
                Assert.IsNotNull(menu);
            }

        }
    }
}
