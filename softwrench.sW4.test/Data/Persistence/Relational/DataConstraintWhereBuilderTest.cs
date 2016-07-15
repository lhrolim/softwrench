using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata.Security;
using qb = softWrench.sW4.Data.Persistence.DefaultValuesBuilder;

namespace softwrench.sW4.test.Data.Persistence.Relational {
    [TestClass]
    public class DataConstraintWhereBuilderTest {
        [TestMethod]
        public void Simple() {
            var user = InMemoryUser.TestInstance();
            user.Login = "test";
            string result = qb.ConvertAllValues("status='WAPPR' and reportedby='@username'", user);
            Assert.AreEqual("status='WAPPR' and reportedby='test'", result);

            result = qb.ConvertAllValues("status='WAPPR' and reportedby=@username", user);
            Assert.AreEqual("status='WAPPR' and reportedby='test'", result);

        }

        [TestMethod]
        public void TestUserGenericProperties() {
            var user = InMemoryUser.TestInstance();
            user.Genericproperties.Add("siteid", "a1");
            user.Genericproperties.Add("orgid", "a2");
            var result = qb.ConvertAllValues("sr.siteid='@user.properties['siteid']' and sr.orgid='@user.properties['orgid']'", user);
            Assert.AreEqual("sr.siteid='a1' and sr.orgid='a2'", result);
        }

        [TestMethod]
        public void SiteId() {
            var user = InMemoryUser.TestInstance();
            user.Login = "test";
            user.SiteId = "testsite";
            string result = qb.ConvertAllValues("status='WAPPR' and reportedby=@username and siteid='@usersite'", user);
            Assert.AreEqual("status='WAPPR' and reportedby='test' and siteid='testsite'", result);



        }

        //        [TestMethod]
        //        public void MultipleAnds()
        //        {
        //            string result = DataConstraintsWhereBuilder.ConvertWhereClause("status in ('WAPPR','APPR') and esttoolcost=100 and (actmatcost='abc' or esttoolcost=200)", "workorder", "test");
        //            Assert.AreEqual("workorder.status in ('WAPPR','APPR') and workorder.esttoolcost=100 and (workorder.actmatcost='abc' or workorder.esttoolcost=200)", result);
        //        }



    }
}
