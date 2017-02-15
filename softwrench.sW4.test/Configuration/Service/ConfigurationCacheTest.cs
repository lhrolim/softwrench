using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Configuration.Services;
using softWrench.sW4.Security.Context;

namespace softwrench.sW4.test.Configuration.Service {
    [TestClass]
    public class ConfigurationCacheTest {

        [TestMethod]
        public void TestCache() {
            var configurationCache = new ConfigurationCache(null);

            var context = new ContextHolder() { ApplicationLookupContext = new ApplicationLookupContext() { MetadataId = "dashboard:chicago.sr.dailytickets" } };
            var context2 = new ContextHolder() { ApplicationLookupContext = new ApplicationLookupContext() { MetadataId = "dashboard:chicago.sr.daily" } };

            configurationCache.AddToCache("a", new ContextHolder() , "1=1");
            configurationCache.AddToCache("a", context, "new");
            configurationCache.AddToCache("a", context2, "new2");

            var result = configurationCache.GetFromCache("a", context);
            Assert.AreEqual("new",result.Value);


        }
    }
}
