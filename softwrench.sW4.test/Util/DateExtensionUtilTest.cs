using System;
using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Metadata;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Util {
    [TestClass]
    public class DateExtensionUtilTest : BaseMetadataTest {

        private DateTime _zeroHour = DateUtil.BeginOfToday();

        [ClassInitialize]
        public static void Init(TestContext testContext) {
            //test_only has no maximoutc configured
            ApplicationConfiguration.TestclientName = "test_only";
            MetadataProvider.StubReset();
        }


        [TestMethod]
        public void TestFromUserToUtc() {
            var utcTime = DateTime.UtcNow;
            var userUtcTime = DateTime.Now.FromUserToUtc();
            Assert.AreEqual(utcTime, userUtcTime);
        }

        [TestMethod]
        public void TestFromMaximoToServer() {
            Assert.AreEqual(_zeroHour.Hour, 0);
            //this simulates Maximo at AZ central and server at local AZ machine --> so something at 0h in maximo was actually saved 23:00 in the server
            const int azCentralOffSet = -6;
            const int azOffSet = 420;//in minutes
            var userTime = DateExtensions.MaximoConversion(_zeroHour, azOffSet, DateExtensions.ConversionKind.MaximoToServer, azCentralOffSet);
            Assert.AreEqual(23, userTime.Hour);
        }

        [TestMethod]
        public void TestFromServerToMaximo() {
            Assert.AreEqual(_zeroHour.Hour, 0);
            //this simulates Maximo at AZ central and server at local AZ machine --> so something saved 0h in AZ should go to maximo as 1AM
            const int azCentralOffSet = -6;
            const int azOffSet = 420;//in minutes
            var userTime = DateExtensions.MaximoConversion(_zeroHour, azOffSet, DateExtensions.ConversionKind.ServerToMaximo, azCentralOffSet);
            Assert.AreEqual(1, userTime.Hour);
        }

        [TestMethod]
        public void TestFromMaximoToUserMockingProperty() {
            Assert.AreEqual(_zeroHour.Hour, 0);
            //this simulates user in BRAZIL and MAXIMO in AZ ==> 4h ahead ==> 12:00AM in arizona means 4AM in BRASIL
            const int brazilOffset = 180;
            var userTime = DateExtensions.MaximoConversion(_zeroHour, brazilOffset, DateExtensions.ConversionKind.MaximoToUser, -7);
            Assert.AreEqual(4, userTime.Hour);
        }

        [TestMethod]
        public void TestFromMaximoToUserReadingFromNullProperty() {
            Assert.AreEqual(_zeroHour.Hour, 0);
            //now, we should considered that maximo is deployed on same timezone as server
            //luiz: need to get server timezone to pass as user timezone, or test would fail in my machine (or on any AZ)
            var serverTimezone = -1 * TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;
            var userTime = DateExtensions.MaximoConversion(_zeroHour, serverTimezone, DateExtensions.ConversionKind.MaximoToUser);
            Assert.AreEqual(0, userTime.Hour);
        }
    }
}