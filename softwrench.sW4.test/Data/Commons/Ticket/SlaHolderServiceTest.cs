using System;
using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Dataset.Commons.Ticket;

namespace softwrench.sW4.test.Data.Commons.Ticket {
    [TestClass]
    public class SlaHolderServiceTest {

        private readonly SlaHolderService service = new SlaHolderService();

        [TestMethod]
        public void TestNullScenario() {
            var dm = DataMap.BlankInstance("sr");
            service.HandleAdjustedTimes(dm);
            //testing no exceptions are thrown

            dm = DataMap.BlankInstance("sr");
            dm.SetAttribute("ACCUMULATEDHOLDTIME", null);
            service.HandleAdjustedTimes(dm);
        }


        [TestMethod]
        public void TestNotAccumulatedScenario() {
            var dm = DataMap.BlankInstance("sr");
            dm.SetAttribute("ACCUMULATEDHOLDTIME", null);
            var refDate = new DateTime(2017, 02, 03, 00, 00, 00);
            dm.SetAttribute("TARGETFINISH", refDate);
            dm.SetAttribute("TARGETSTART", refDate);
            service.HandleAdjustedTimes(dm);

            var adjustedTarget = (DateTime)dm.GetAttribute("ADJUSTEDTARGETRESOLUTIONTIME");
            var adjustedResponse = (DateTime)dm.GetAttribute("ADJUSTEDTARGETRESPONSETIME");

            Assert.AreEqual(adjustedResponse,refDate);
            Assert.AreEqual(adjustedTarget, refDate);

            
        }



        [TestMethod]
        public void TestGeneratedDates() {
            var dm = DataMap.BlankInstance("sr");
            dm.SetAttribute("ACCUMULATEDHOLDTIME", 32.40);
            var refDate = new DateTime(2017, 02, 03, 00, 00, 00);
            dm.SetAttribute("TARGETSTART", refDate);
            dm.SetAttribute("ADJUSTEDTARGETRESOLUTIONTIME", null);
            dm.SetAttribute("ADJUSTEDTARGETRESPONSETIME", null);
            service.HandleAdjustedTimes(dm);
            var newDate = (DateTime)dm.GetAttribute("ADJUSTEDTARGETRESPONSETIME");
            Assert.AreEqual(newDate, refDate);
        }


        [TestMethod]
        public void TestGeneratedDatesEdgyMinutesScenario() {
            var dm = DataMap.BlankInstance("sr");
            dm.SetAttribute("ACCUMULATEDHOLDTIME", 32.999698519706726);
            var refDate = new DateTime(2017, 02, 03, 00, 00, 00);
            dm.SetAttribute("TARGETFINISH", refDate);
            service.HandleAdjustedTimes(dm);
            var newDate = (DateTime)dm.GetAttribute("ADJUSTEDTARGETRESOLUTIONTIME");
            Assert.AreNotEqual(newDate, refDate);
            Assert.AreEqual(newDate.Day, 04);
            Assert.AreEqual(newDate.Hour, 09);
            Assert.AreEqual(newDate.Minute, 0);
        }


        [TestMethod]
        public void TestAlreadyPersistedScenario() {
            var dm = DataMap.BlankInstance("sr");
            dm.SetAttribute("ACCUMULATEDHOLDTIME", 32.999698519706726);
            var refDate = new DateTime(2017, 02, 03, 00, 00, 00);
            dm.SetAttribute("ADJUSTEDTARGETRESPONSETIME", refDate);
            dm.SetAttribute("TARGETFINISH", refDate);
            dm.SetAttribute("TARGETSTART", refDate);
            service.HandleAdjustedTimes(dm);

            var newDate = (DateTime)dm.GetAttribute("ADJUSTEDTARGETRESPONSETIME");
            Assert.AreEqual(newDate, refDate);
            

            var newStartDate = (DateTime)dm.GetAttribute("ADJUSTEDTARGETRESOLUTIONTIME");
            Assert.AreNotEqual(newStartDate, refDate);
            Assert.AreEqual(newStartDate.Day, 04);
            Assert.AreEqual(newStartDate.Hour, 09);
            Assert.AreEqual(newStartDate.Minute, 0);
        }
    }
}
