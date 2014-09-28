using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Pagination;

namespace softwrench.sW4.test
{
    [TestClass]
    public class PaginationUtilsTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            Tuple<int, int> result = PaginationUtils.GetPaginationBounds(9, 10);
            Assert.AreEqual(1, result.Item1);
            Assert.AreEqual(10, result.Item2);
        }

        [TestMethod]
        public void TestMethod2()
        {
            Tuple<int, int> result = PaginationUtils.GetPaginationBounds(2, 10);
            Assert.AreEqual(1, result.Item1);
            Assert.AreEqual(10, result.Item2);
        }


        [TestMethod]
        public void TestMethod3()
        {
            Tuple<int, int> result = PaginationUtils.GetPaginationBounds(9, 15);
            Assert.AreEqual(4, result.Item1);
            Assert.AreEqual(13, result.Item2);
        }

        [TestMethod]
        public void TestMethod4()
        {
            Tuple<int, int> result = PaginationUtils.GetPaginationBounds(1, 10);
            Assert.AreEqual(1, result.Item1);
            Assert.AreEqual(10, result.Item2);
        }

    }
}
