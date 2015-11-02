using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Scheduler.Jobs;
using softWrench.sW4.Scheduler.Jobs.helper;

namespace softwrench.sW4.test.Scheduler.Jobs {
    [TestClass]
    public class R101ExtractorHelperTest {


        internal const string ExampleResult = @"asset.CLASSSTRUCTUREID in ('43211503','43211507','43211508','43211509')";


        [TestMethod]
        public void R101DefaultClause() {
            var result = R101ExtractorHelper.DoBuildQuery(new[]
            {
                @"43211503",
                @"43211507",
                @"43211508",
                @"43211509"
            });
            Assert.AreEqual(ExampleResult, result);



        }
    }
}
