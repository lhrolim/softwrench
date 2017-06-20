﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softwrench.sw4.firstsolar.classes.com.cts.firstsolar.model;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Util {
    [TestClass]
    public class ReflectionUtilTest {

        [TestMethod]
        public void TestMethod1() {
            var workpackage = new WorkPackage();
            ReflectionUtil.SetProperty(workpackage, "id", "1");
            Assert.AreEqual(workpackage.Id, 1);

            ReflectionUtil.SetProperty(workpackage, "id", "1");
            Assert.AreEqual(workpackage.Id, 1);

            ReflectionUtil.SetProperty(workpackage, "TestResultReviewEnabled", true);
            Assert.AreEqual(workpackage.TestResultReviewEnabled, true);

            ReflectionUtil.SetProperty(workpackage, "TestResultReviewEnabled", "1");
            Assert.AreEqual(workpackage.TestResultReviewEnabled, true);

            ReflectionUtil.SetProperty(workpackage, "TestResultReviewEnabled", "False");
            Assert.AreEqual(workpackage.TestResultReviewEnabled, false);
        }
    }
}