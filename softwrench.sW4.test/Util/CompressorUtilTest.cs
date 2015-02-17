using System;
using cts.commons.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Util;

namespace softwrench.sW4.test.Util {

    [TestClass]
    public class CompressorUtilTest {
        [TestMethod]
        public void TestStringToBytes()
        {
            const string a = "abcd";
            var bytes =a.GetBytes();
            Assert.AreEqual(a, StringExtensions.GetString(bytes));
        }
        [TestMethod]
        public void TestStringToBytesCompressed() {
            const string a = "abcd";
            var bytes = a.GetBytes();
            var compressed =CompressionUtil.Compress(bytes);
            var decompressed = CompressionUtil.Decompress(compressed);
            Assert.AreEqual(a, StringExtensions.GetString(decompressed));
        }
    }
}
