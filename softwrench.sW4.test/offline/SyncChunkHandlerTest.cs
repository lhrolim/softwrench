using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.offlineserver.dto.association;
using softwrench.sw4.offlineserver.services;
using softwrench.sw4.offlineserver.services.util;
using softwrench.sW4.test.Util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;

namespace softwrench.sW4.test.offline {

    [TestClass]
    public class SyncChunkHandlerTest {

        private readonly Mock<IConfigurationFacade> _configMock = TestUtil.CreateMock<IConfigurationFacade>();

        private SyncChunkHandler _syncChunkHandler;

        [TestInitialize]
        public void Init() {
            TestUtil.ResetMocks(_configMock);
            _syncChunkHandler = new SyncChunkHandler(_configMock.Object);
        }

        [TestMethod]
        public async Task TestSyncLimit() {
            var results = new AssociationSynchronizationResultDto();
            results.AssociationData["asset"] = BlankInstances("asset", 5);
            results.AssociationData["location"] = BlankInstances("location",10);
            results.AssociationData["syndomain"] = BlankInstances("syndomain", 20);

            _configMock.Setup(c => c.LookupAsync<int>(OfflineConstants.MaxDownloadSize, null)).ReturnsAsync(10);


            var newResult = await _syncChunkHandler.HandleMaxSize(results);
            Assert.IsTrue(newResult.HasMoreData);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("asset"));
            Assert.AreEqual(5, newResult.AssociationData["asset"].Count);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("location"));
            Assert.AreEqual(5, newResult.AssociationData["location"].Count);
            Assert.IsFalse(newResult.AssociationData.ContainsKey("syndomain"));

            Assert.AreEqual(2, newResult.IncompleteAssociations.Count);
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("location"));
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("syndomain"));

            TestUtil.VerifyMocks(_configMock);

        }


        [TestMethod]
        public async Task TestSyncLimit2() {
            var results = new AssociationSynchronizationResultDto();
            results.AssociationData["asset"] = BlankInstances("asset", 0);
            results.AssociationData["yyy"] = BlankInstances("yyy", 0);
            results.AssociationData["location"] = BlankInstances("location", 10);
            results.AssociationData["syndomain"] = BlankInstances("syndomain", 20);

            _configMock.Setup(c => c.LookupAsync<int>(OfflineConstants.MaxDownloadSize, null)).ReturnsAsync(10);


            var newResult = await _syncChunkHandler.HandleMaxSize(results);
            Assert.IsTrue(newResult.HasMoreData);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("location"));
            Assert.IsTrue(newResult.AssociationData.ContainsKey("asset"));
            Assert.IsTrue(newResult.AssociationData.ContainsKey("yyy"));

            Assert.AreEqual(1, newResult.IncompleteAssociations.Count);
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("syndomain"));
            
            Assert.AreEqual(10, newResult.AssociationData["location"].Count);
            Assert.IsFalse(newResult.AssociationData.ContainsKey("syndomain"));
            TestUtil.VerifyMocks(_configMock);

        }


        [TestMethod]
        public async Task TestSyncLimitFirstBigger() {
            var results = new AssociationSynchronizationResultDto();
            results.AssociationData["asset"] = BlankInstances("asset", 15);
            results.AssociationData["location"] = BlankInstances("location", 10);
            results.AssociationData["syndomain"] = BlankInstances("syndomain", 20);

            _configMock.Setup(c => c.LookupAsync<int>(OfflineConstants.MaxDownloadSize, null)).ReturnsAsync(10);


            var newResult = await _syncChunkHandler.HandleMaxSize(results);
            Assert.IsTrue(newResult.HasMoreData);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("asset"));
            Assert.AreEqual(10, newResult.AssociationData["asset"].Count);
            Assert.IsFalse(newResult.AssociationData.ContainsKey("location"));
            Assert.IsFalse(newResult.AssociationData.ContainsKey("syndomain"));

            Assert.AreEqual(3, newResult.IncompleteAssociations.Count);

            Assert.IsTrue(newResult.IncompleteAssociations.Contains("location"));
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("asset"));
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("syndomain"));


            TestUtil.VerifyMocks(_configMock);

        }


        private static List<DataMap> BlankInstances(string applicationName,int limit)
        {
            var results = new List<DataMap>();
            for (var i = 0; i < limit; i++) {
                results.Add(DataMap.BlankInstance(applicationName));
            }

            return results;;
        }
    }
}
