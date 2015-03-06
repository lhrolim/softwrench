using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata;
using softWrench.sW4.Security.Services;
using softWrench.sW4.Util;
using softWrench.sW4.Metadata.Security;

namespace softwrench.sW4.test.Util {
    [TestClass]
    public class DateExtensionUtilTest {
        [TestMethod]
        public void TestFromUserToUtc() {
            var utcTime = DateTime.UtcNow;
            var userUtcTime = DateTime.Now.FromUserToUtc(SecurityFacade.CurrentUser());
            Assert.AreEqual(utcTime, userUtcTime);
        }

        [TestMethod]
        public void TestFromMaximoToServer() {
            const string midnight = "12:00 am";
            MetadataProvider.InitializeMetadata();
            // Get maximo offset
            var maximoUtcProp = MetadataProvider.GlobalProperties.MaximoTimeZone();
            int maximoUtc;
            if (!Int32.TryParse(maximoUtcProp, out maximoUtc)) {
                maximoUtc = 0;
            }
            var maximoOffset = maximoUtc * 60;
            // Set maximo time as midnight
            var midnightMaximo = DateTime.Parse(midnight);
            // Get server offset
            double serverOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;

            // Set server time to maximo midnight as server
            var serverTime = midnightMaximo.FromMaximoToServer();

            // Get difference in minutes between maximo time and server time
            var difference = (midnightMaximo - serverTime).TotalMinutes;
            // Difference should be the same as the difference between offset's
            Assert.AreEqual(difference, maximoOffset - serverOffset);
        }

        [TestMethod]
        public void TestFromMaximoToUser()
        {
            const string midnight = "12:00 am";
            MetadataProvider.InitializeMetadata();
            // Get maximo offset
            var maximoUtcProp = MetadataProvider.GlobalProperties.MaximoTimeZone();
            int maximoUtc;
            if (!Int32.TryParse(maximoUtcProp, out maximoUtc)) {
                maximoUtc = 0;
            }
            double maximoOffset = maximoUtc * 60;
            // Set maximo time as midnight
            var midnightMaximo = DateTime.Parse(midnight);
            // Get user offset
            int? userOffset = SecurityFacade.CurrentUser().TimezoneOffset;
            
            // Set user time from maximo midnight utc
            var userTime = midnightMaximo.FromMaximoToUser(SecurityFacade.CurrentUser());

            // Get difference between user and maximo
            double difference = (midnightMaximo - userTime).TotalMinutes;
            // Difference should be the same as the difference between the two offset's
            Assert.AreEqual(difference, maximoOffset - userOffset);
        }
    }
}