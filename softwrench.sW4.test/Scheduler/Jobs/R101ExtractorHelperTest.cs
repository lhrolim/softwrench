using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Scheduler.Jobs;
using softwrench.sw4.Hapag.Data.Scheduler.Jobs.Helper;

namespace softwrench.sW4.test.Scheduler.Jobs {
    [TestClass]
    public class R101ExtractorHelperTest {


        internal const string ExampleResult = @"asset.CLASSSTRUCTUREID in ('43211503','43211507','43211508','43211509')";

        internal const string ExampleResultWithParent = @"(asset.CLASSSTRUCTUREID in ('43211503','43211507','43211508','43211509','43211501') or asset.CLASSSTRUCTUREID in (select c2.CLASSSTRUCTUREID from maximo.CLASSSTRUCTURE c1 inner join maximo.CLASSSTRUCTURE c2 on c2.PARENT = c1.CLASSSTRUCTUREID and c1.CLASSSTRUCTUREID in ('43211500','43211400')))";

        internal const string ExampleResultWithParentOnly = @"asset.CLASSSTRUCTUREID in (select c2.CLASSSTRUCTUREID from maximo.CLASSSTRUCTURE c1 inner join maximo.CLASSSTRUCTURE c2 on c2.PARENT = c1.CLASSSTRUCTUREID and c1.CLASSSTRUCTUREID in ('43211500'))";


        [TestMethod]
        public void R101DefaultClause() {
            var result = ClassStructureConfigFileReader.DoBuildQuery(new[]
            {
                @"43211503",
                @"43211507",
                @"43211508",
                @"43211509"
            });
            Assert.AreEqual(ExampleResult, result);
        }


        [TestMethod]
        public void R101WithParentsAndChildren() {
            var result = ClassStructureConfigFileReader.DoBuildQuery(new[]
            {
                @"43211503",
                @"43211507",
                @"43211508",
                @"43211509",
                @"43211500\",
                @"43211501",
                @"43211400\"
            });
            Assert.AreEqual(ExampleResultWithParent, result);
        }


        [TestMethod]
        public void R101WithParentOnly() {
            var result = ClassStructureConfigFileReader.DoBuildQuery(new[]
            {
                @"43211500\"
            });
            Assert.AreEqual(ExampleResultWithParentOnly, result);
        }
    }
}
