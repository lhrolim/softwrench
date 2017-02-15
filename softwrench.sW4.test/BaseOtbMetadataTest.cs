using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;
using softwrench.sW4.TestBase;

namespace softwrench.sW4.test {
    [TestClass]
    public abstract class BaseOtbMetadataTest : BaseMetadataTest {

        [TestInitialize]
        public virtual void Init() {
            if (ApplicationConfiguration.TestclientName != GetClientName()) {
                ApplicationConfiguration.TestclientName = GetClientName();
                MetadataProvider.StubReset();
            }
        }

        public virtual string GetClientName() {
            return "otb";
        }

    }
}
