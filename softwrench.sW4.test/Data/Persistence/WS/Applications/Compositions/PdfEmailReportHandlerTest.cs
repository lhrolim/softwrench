using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;

namespace softwrench.sW4.test.Data.Persistence.WS.Applications.Compositions {

    [TestClass]
    public class PdfEmailReportHandlerTest {

        [TestMethod]
        public void TestMethod1() {
            var processed = PdfEmailReportHandler.ProcessHtml("<link href=\"/softwrench/Content/dist/css/vendor.css?1463063880000\" rel=\"stylesheet\">", "softwrench", "C:\\path");
            Assert.AreEqual("<link href=\"C:\\path/Content/dist/css/vendor.css?1463063880000\" rel=\"stylesheet\">", processed);
        }

        [TestMethod]
        public void TestMethod2() {
            var processed = PdfEmailReportHandler.ProcessHtml("<link href=\"/Content/dist/css/vendor.css?1463063880000\" rel=\"stylesheet\">", "softwrench", "C:\\path");
            Assert.AreEqual("<link href=\"C:\\path/Content/dist/css/vendor.css?1463063880000\" rel=\"stylesheet\">", processed);
        }
    }
}
