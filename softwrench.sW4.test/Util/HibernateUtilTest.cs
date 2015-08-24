using cts.commons.persistence.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace softwrench.sW4.test.Util {

    [TestClass]
    public class HibernateUtilTest {
        [TestMethod]
        public void TestQueryTranslation() {
            var result = HibernateUtil.TranslateQueryString(
                "from PropertyValue v where Definition = ? and Condition = ? and Module = ? and UserProfile = ?", 1, null, null, 2);
            Assert.AreEqual("from PropertyValue v where Definition = ? and Condition  is   null  and Module  is   null  and UserProfile = ?", result.query);
            Assert.AreEqual(2, result.Parameters.Length);

            result = HibernateUtil.TranslateQueryString(
                "from PropertyValue v where Definition = ? and Condition = ? and Module = ? and UserProfile = ?", 1, 2, 2, 2);
            Assert.AreEqual("from PropertyValue v where Definition = ? and Condition = ? and Module = ? and UserProfile = ?", result.query);
            Assert.AreEqual(4, result.Parameters.Length);

            result = HibernateUtil.TranslateQueryString(
                            "from PropertyValue v where Definition = ? and Condition = ? and Module = ? and UserProfile = ?   ", 1, null, null, 2);
            Assert.AreEqual("from PropertyValue v where Definition = ? and Condition  is   null  and Module  is   null  and UserProfile = ?   ", result.query);
            Assert.AreEqual(2, result.Parameters.Length);

            result = HibernateUtil.TranslateQueryString(
                            "from PropertyValue");
            Assert.AreEqual("from PropertyValue", result.query);

        }
    }
}
