using System;
using System.Collections.Generic;
using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Email;

namespace softwrench.sW4.test.Email {
    [TestClass]
    public class CommLogTemplateMergerTest {

        private string testData1 = @"
You can also track your request at http://www.kogtsupport24.com

Thank You,

:OWNERPERSON.displayname
Support24 America: +1.866.994.7765
Support24 Norway: +47.4000.1024
Support24 Australia: +61.861.413.355
Support24 UK: +44.1224.226.583
Support24 Email: support@kogtsupport24.com

:description was last modified on :changedate

:description_longdescription";

        private string resultData1 = @"
You can also track your request at http://www.kogtsupport24.com

Thank You,

{0}
Support24 America: +1.866.994.7765
Support24 Norway: +47.4000.1024
Support24 Australia: +61.861.413.355
Support24 UK: +44.1224.226.583
Support24 Email: support@kogtsupport24.com

{1} was last modified on {2}

{3}";

        [TestMethod]
        public void TestVariableLocation() {
            var merger = new CommLogTemplateMerger(null);
            var list = new List<string>(merger.LocateVariables(testData1));
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual("ownerperson.displayname", list[0]);
            Assert.AreEqual("description", list[1]);
            Assert.AreEqual("changedate", list[2]);
            Assert.AreEqual("description_longdescription", list[3]);
        }

        [TestMethod]
        public void TestVariableLocation2() {
            var merger = new CommLogTemplateMerger(null);
            var list = new List<string>(merger.LocateVariables("ftp://xLedaFlowDL-121:Leda12!34Flow@ftp.km.kongsberg.com/LedaFlow%20Engineering/v1.7/v1.7.248.921/"));
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void TestVariableLocation3() {
            var merger = new CommLogTemplateMerger(null);
            var list = new List<string>(merger.LocateVariables("<font>:description_longdescription"));
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void TestMergeTemplateDefinition() {
            var merger = new CommLogTemplateMerger(null);
            IDictionary<string, string> dict = new Dictionary<string, string>
            {
                {"ownerperson.displayname", "a"},
                {"description", "b"},
                {"changedate", "c"},
                {"description_longdescription", "d"},

            };

            var result = merger.MergeTemplateDefinition(testData1, dict);
            Assert.AreEqual(resultData1.Fmt("a", "b", "c", "d"), result);

        }
    }
}
