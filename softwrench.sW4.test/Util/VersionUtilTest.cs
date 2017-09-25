﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Util {
    [TestClass]
    public class VersionUtilTest {

        [TestMethod]
        public void CompareVersions() {
            Assert.IsTrue(VersionUtil.IsGreaterThan("3.8.0", "3.7.0"));
            Assert.IsTrue(VersionUtil.IsGreaterThan("3.8", "3.7.0"));
            Assert.IsFalse(VersionUtil.IsGreaterThan("3.6.5", "3.7.0"));
            Assert.IsFalse(VersionUtil.IsGreaterThan("3.6.5", "3.7.0,3.7.1"));
            Assert.IsTrue(VersionUtil.IsGreaterThan("3.8.8", "3.7.0,3.7.1"));
            Assert.IsTrue(VersionUtil.IsGreaterThan("3.9-SNAPSHOT", "3.7.0"));
            Assert.IsTrue(VersionUtil.IsGreaterThan("3.9-SNAPSHOT#17042013996ccb0ef4261003e23fe7c053c054b7", "3.7.0"));
            Assert.IsTrue(VersionUtil.IsGreaterThan("3.9-SNAPSHOT#17042013996ccb0ef4261003e23fe7c053c054b7", ""));
        }
    }
}