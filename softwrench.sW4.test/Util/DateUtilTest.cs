using System;
using cts.commons.portable.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace softwrench.sW4.test.Util {

    [TestClass]
    public class DateUtilTest {

        private static DateTime _baseDate;

        private static void BuildBaseDate() {
            _baseDate = new DateTime(1987, 11, 16, 23, 30, 27);
            _baseDate = _baseDate.AddMilliseconds(123);
        }

        private void BaseTest(int number, DateTime expected) {
            BuildBaseDate();
            var result = DateUtil.ProcessWeeks(_baseDate, number);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ProcessWeekZeroTest() {
            BuildBaseDate();
            var result = DateUtil.ProcessWeeks(_baseDate, 0);
            Assert.AreEqual(_baseDate, result);
        }

        [TestMethod]
        public void ProcessWeekPastTest() {
            BaseTest(-1, new DateTime(1987, 11, 15));
        }

        [TestMethod]
        public void ProcessWeekFutureTest() {
            BaseTest(1, new DateTime(1987, 11, 22));
        }

        [TestMethod]
        public void ProcessWeekPastManyTest() {
            BaseTest(-5, new DateTime(1987, 10, 18));
        }

        [TestMethod]
        public void ProcessWeekFutureManyTest() {
            BaseTest(3, new DateTime(1987, 12, 6));
        }
    }
}
