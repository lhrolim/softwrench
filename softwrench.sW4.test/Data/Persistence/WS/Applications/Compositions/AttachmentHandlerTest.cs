using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using softWrench.sW4.Data.Persistence.WS.Applications.Compositions;

namespace softwrench.sW4.test.Data.Persistence.WS.Applications.Compositions {
    [TestClass]
    public class AttachmentHandlerTest
    {
        private const string FileName = "b54a2634755c5ba2733785d55794e2a1.jpg";
        private const string MultipleFileNames = "b54a2634755c5ba2733785d55794e2a1.jpg,b54a2634755c5ba2733785d55794e2a1.jpg";

        private string BaseOffline64Data = @"Wy5TaGVsbENsYXNzSW5mb10NCkluZm9UaXA9VGhpcyBmb2xkZXIgaXMgc2hhcmVkIG9ubGluZS4N
                                     Ckljb25GaWxlPUM6XFByb2dyYW0gRmlsZXMgKHg4NilcR29vZ2xlXERyaXZlXGdvb2dsZWRyaXZl
                                     c3luYy5leGUNCkljb25JbmRleD0xNg0KICAgIA==";

        private string BaseOnline64Data = @"data:application/pdf;base64,Wy5TaGVsbENsYXNzSW5mb10NCkluZm9UaXA9VGhpcyBmb2xkZXIgaXMgc2hhcmVkIG9ubGluZS4N
                                     Ckljb25GaWxlPUM6XFByb2dyYW0gRmlsZXMgKHg4NilcR29vZ2xlXERyaXZlXGdvb2dsZWRyaXZl
                                     c3luYy5leGUNCkljb25JbmRleD0xNg0KICAgIA==";


        private string BaseOnline64MultipleData = @"data:application/pdf;base64,Wy5T,data:application/pdf;base64,Wy54";


        [TestMethod]
        public void TestSingleAttachmentOffline() {
            var dtos = new AttachmentHandler(null, null, null,null).BuildAttachments(FileName, BaseOffline64Data, FileName,null, "hash");
            Assert.AreEqual(1, dtos.Count);
            var dto = dtos[0];
            Assert.AreEqual("hash",dto.OffLineHash);
            Assert.AreEqual(BaseOffline64Data, dto.Data);
            Assert.AreEqual(FileName, dto.Path);
        }

        [TestMethod]
        public void TestSingleAttachmentOnline() {
            var dtos = new AttachmentHandler(null, null, null, null).BuildAttachments(FileName, BaseOnline64Data, FileName);
            Assert.AreEqual(1, dtos.Count);
            var dto = dtos[0];
            Assert.AreEqual(BaseOnline64Data, dto.Data);
            Assert.AreEqual(FileName, dto.Path);
        }

        [TestMethod]
        public void TestMultiplesAttachmentOnline() {
            var dtos = new AttachmentHandler(null, null, null, null).BuildAttachments(MultipleFileNames, BaseOnline64MultipleData, MultipleFileNames);
            Assert.AreEqual(2, dtos.Count);
        }
    }
}
