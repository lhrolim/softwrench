using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.offlineserver.model.dto.association;
using softwrench.sw4.offlineserver.services;
using softwrench.sw4.offlineserver.services.util;
using softwrench.sW4.test.Util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;

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
            var results = new AssociationSynchronizationResultDto(10);
            results.AssociationData["asset"] = BlankInstances("asset", 5);
            results.AssociationData["location"] = BlankInstances("location", 10);
            results.AssociationData["syndomain"] = BlankInstances("syndomain", 20);

            _configMock.Setup(c => c.LookupAsync<int>(OfflineConstants.MaxDownloadSize, null)).ReturnsAsync(10);


            var newResult = await _syncChunkHandler.HandleMaxSize(results, true);
            Assert.IsTrue(newResult.HasMoreData);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("asset"));
            Assert.AreEqual(5, newResult.AssociationData["asset"].Count);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("location"));
            Assert.AreEqual(5, newResult.AssociationData["location"].Count);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("syndomain"));
            Assert.AreEqual(0, newResult.AssociationData["syndomain"].Count);

            Assert.AreEqual(2, newResult.AssociationData.Values.Where(a => a.Incomplete).ToList().Count);
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("location"));
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("syndomain"));

            TestUtil.VerifyMocks(_configMock);

        }


        [TestMethod]
        public async Task TestSyncLimit2() {
            var results = new AssociationSynchronizationResultDto(10);
            results.AssociationData["asset"] = BlankInstances("asset", 0);
            results.AssociationData["yyy"] = BlankInstances("yyy", 0);
            results.AssociationData["location"] = BlankInstances("location", 10);
            results.AssociationData["syndomain"] = BlankInstances("syndomain", 20);

            _configMock.Setup(c => c.LookupAsync<int>(OfflineConstants.MaxDownloadSize, null)).ReturnsAsync(10);


            var newResult = await _syncChunkHandler.HandleMaxSize(results,true);
            Assert.IsTrue(newResult.HasMoreData);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("location"));
            Assert.IsTrue(newResult.AssociationData.ContainsKey("asset"));
            Assert.IsTrue(newResult.AssociationData.ContainsKey("yyy"));

            Assert.AreEqual(1, newResult.IncompleteAssociations.Count);
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("syndomain"));

            Assert.AreEqual(10, newResult.AssociationData["location"].Count);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("syndomain"));
            TestUtil.VerifyMocks(_configMock);

        }


        [TestMethod]
        public async Task TestSyncLimitFirstBigger() {
            var results = new AssociationSynchronizationResultDto(10);
            results.AssociationData["asset"] = BlankInstances("asset", 15);
            results.AssociationData["location"] = BlankInstances("location", 10);
            results.AssociationData["syndomain"] = BlankInstances("syndomain", 20);

            _configMock.Setup(c => c.LookupAsync<int>(OfflineConstants.MaxDownloadSize, null)).ReturnsAsync(10);


            var newResult = await _syncChunkHandler.HandleMaxSize(results,true);
            Assert.IsTrue(newResult.HasMoreData);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("asset"));
            Assert.AreEqual(10, newResult.AssociationData["asset"].Count);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("location"));
            Assert.IsTrue(newResult.AssociationData.ContainsKey("syndomain"));
            Assert.AreEqual(0, newResult.AssociationData["location"].Count);
            Assert.AreEqual(0, newResult.AssociationData["syndomain"].Count);

            Assert.AreEqual(3, newResult.IncompleteAssociations.Count);

            Assert.IsTrue(newResult.IncompleteAssociations.Contains("location"));
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("asset"));
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("syndomain"));


            TestUtil.VerifyMocks(_configMock);

        }


        [TestMethod]
        public async Task TestSyncLimitMultipleKeys() {
            var results = new AssociationSynchronizationResultDto(10);
            results.AssociationData["asset"] = BlankInstances("asset", 7);
            results.AssociationData["location"] = BlankInstances("location", 10);
            var locationData = results.AssociationData["location"];
            locationData.CompleteCacheEntries = new Dictionary<string, CacheRoundtripStatus>{
                {"1", new CacheRoundtripStatus {TransientPosition = 2, Position = 4}},
                {"2", new CacheRoundtripStatus {TransientPosition = 5}},
                {"3", new CacheRoundtripStatus {TransientPosition = 5}},
            };


            _configMock.Setup(c => c.LookupAsync<int>(OfflineConstants.MaxDownloadSize, null)).ReturnsAsync(10);


            var newResult = await _syncChunkHandler.HandleMaxSize(results, true);
            Assert.IsTrue(newResult.HasMoreData);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("asset"));
            Assert.AreEqual(7, newResult.AssociationData["asset"].Count);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("location"));
            Assert.AreEqual(3, newResult.AssociationData["location"].Count);

            //location is incomplete
            Assert.AreEqual(1, newResult.IncompleteAssociations.Count);
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("location"));

            Assert.IsTrue(newResult.CompleteCacheEntries["1"].Complete);
            Assert.IsFalse(newResult.CompleteCacheEntries["2"].Complete);
            Assert.IsFalse(newResult.CompleteCacheEntries["3"].Complete);

            Assert.AreEqual(newResult.CompleteCacheEntries["1"].Position,6);
            Assert.AreEqual(newResult.CompleteCacheEntries["2"].Position,1);
            Assert.AreEqual(newResult.CompleteCacheEntries["3"].Position,0);


            TestUtil.VerifyMocks(_configMock);

        }

        [TestMethod]
        public async Task TestSyncLimitMultipleKeys2() {
            var results = new AssociationSynchronizationResultDto(10);
            results.AssociationData["asset"] = BlankInstances("asset", 7);
            results.AssociationData["location"] = BlankInstances("location", 10);
            var locationData = results.AssociationData["location"];
            locationData.CompleteCacheEntries = new Dictionary<string, CacheRoundtripStatus>{
                {"1", new CacheRoundtripStatus {Position = 4, Complete = true}},
                {"2", new CacheRoundtripStatus {TransientPosition = 5}},
                {"3", new CacheRoundtripStatus {TransientPosition = 5}},
            };


            _configMock.Setup(c => c.LookupAsync<int>(OfflineConstants.MaxDownloadSize, null)).ReturnsAsync(10);


            var newResult = await _syncChunkHandler.HandleMaxSize(results, true);
            Assert.IsTrue(newResult.HasMoreData);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("asset"));
            Assert.AreEqual(7, newResult.AssociationData["asset"].Count);
            Assert.IsTrue(newResult.AssociationData.ContainsKey("location"));
            Assert.AreEqual(3, newResult.AssociationData["location"].Count);

            //location is incomplete
            Assert.AreEqual(1, newResult.IncompleteAssociations.Count);
            Assert.IsTrue(newResult.IncompleteAssociations.Contains("location"));

            Assert.IsTrue(newResult.CompleteCacheEntries["1"].Complete);
            Assert.IsFalse(newResult.CompleteCacheEntries["2"].Complete);
            Assert.IsFalse(newResult.CompleteCacheEntries["3"].Complete);

            Assert.AreEqual(newResult.CompleteCacheEntries["1"].Position, 4);
            Assert.AreEqual(newResult.CompleteCacheEntries["2"].Position, 3);
            Assert.AreEqual(newResult.CompleteCacheEntries["3"].Position, 0);


            TestUtil.VerifyMocks(_configMock);

        }


        private static AssociationDataDto BlankInstances(string applicationName, int limit) {
            var dto = new AssociationDataDto();

            var results = new List<DataMap>();
            for (var i = 0; i < limit; i++) {
                results.Add(DataMap.BlankInstance(applicationName));
            }
            dto.IndividualItems = results;
            return dto;

        }
    }
}
