using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using softwrench.sw4.offlineserver.dto;
using softwrench.sW4.Shared2.Metadata.Applications.Schema;
using softwrench.sW4.test.Util;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;
using softWrench.sW4.Data.Persistence.Relational.Cache.Core;
using StackExchange.Redis.Extensions.Core;

namespace softwrench.sW4.test.Data.Persistence.Relational.Cache {

    [TestClass]
    public class RedisManagerUnitTest {

        private readonly RedisManager _redisManager = new RedisManager(null);

        private readonly Mock<ICacheClient> _cacheClient = TestUtil.CreateMock<ICacheClient>(true);



        [TestInitializeAttribute]
        public void Init() {
            TestUtil.ResetMocks(_cacheClient);
            _redisManager.CacheClient = _cacheClient.Object;
        }


        #region groupByDatamap


        [TestMethod]
        public void GroupByChunkTest_NO_UPDATES() {

            //already one chunk in place
            var chunks = new List<RedisLookupRowstampChunkHash>{
                new RedisLookupRowstampChunkHash{
                    RealKey = "xxx;chunk:1",
                    Count = 10,
                    MinUid = 1,
                    MaxUid = 10
                }
            };

            var descriptor = new RedisChunkMetadataDescriptor {
                Chunks = chunks
            };
            var datamaps = GenerateListOfDataMaps(15, 10);
            var input = new RedisInputDTO<JSONConvertedDatamap>(datamaps);
            var results = _redisManager.GroupDatamapsByChunk(descriptor, input, 10).Results;
            Assert.AreEqual(3, results.Count);

            var firstItem = results[0];
            var secondItem = results[1];
            var thirdItem = results[2];
            BaseAssertions(results);

            Assert.AreEqual(10, firstItem.Metadata.Count);
            Assert.AreEqual(10, secondItem.Metadata.Count);
            Assert.AreEqual(5, thirdItem.Metadata.Count);

            Assert.AreEqual(11, secondItem.Metadata.MinUid);
            Assert.AreEqual(20, secondItem.Metadata.MaxUid);

            Assert.AreEqual(21, thirdItem.Metadata.MinUid);
            Assert.AreEqual(25, thirdItem.Metadata.MaxUid);


        }

        [TestMethod]
        public void GroupByChunkTest_NO_UPDATES_2CHUNKS() {

            //already one chunk in place
            var chunks = new List<RedisLookupRowstampChunkHash>{
                new RedisLookupRowstampChunkHash{
                    RealKey = "xxx;chunk:1",
                    Count = 10,
                    MinUid = 1,
                    MaxUid = 10
                },
                new RedisLookupRowstampChunkHash{
                    RealKey = "xxx;chunk:2",
                    Count = 5,
                    MinUid = 11,
                    MaxUid = 15
                }
            };

            var descriptor = new RedisChunkMetadataDescriptor {
                Chunks = chunks
            };
            var datamaps = GenerateListOfDataMaps(5, 15);
            var input = new RedisInputDTO<JSONConvertedDatamap>(datamaps);
            var results = _redisManager.GroupDatamapsByChunk(descriptor, input, 10).Results;
            Assert.AreEqual(2, results.Count);

            var firstItem = results[0];
            var secondItem = results[1];
            BaseAssertions(results);

            Assert.AreEqual(10, firstItem.Metadata.Count);
            Assert.AreEqual(10, secondItem.Metadata.Count);

            Assert.AreEqual(0, firstItem.ToInsert.Count);
            Assert.AreEqual(0, firstItem.ToUpdate.Count);

            Assert.AreEqual(5, secondItem.ToInsert.Count);
            Assert.AreEqual(0, secondItem.ToUpdate.Count);

            Assert.AreEqual(11, secondItem.Metadata.MinUid);
            Assert.AreEqual(20, secondItem.Metadata.MaxUid);
        }


        [TestMethod]
        public void GroupByChunkTest_NO_UPDATES_LAST_CHUNK_INCOMPLETE() {

            //already one chunk in place
            var chunks = new List<RedisLookupRowstampChunkHash>{
                new RedisLookupRowstampChunkHash{
                    RealKey = "xxx;chunk:1",
                    Count = 8,
                    MinUid = 1,
                    MaxUid = 8
                }
            };

            var descriptor = new RedisChunkMetadataDescriptor {
                Chunks = chunks
            };
            var datamaps = GenerateListOfDataMaps(15, 8);
            var input = new RedisInputDTO<JSONConvertedDatamap>(datamaps);
            var results = _redisManager.GroupDatamapsByChunk(descriptor, input, 10).Results;
            Assert.AreEqual(3, results.Count);

            var firstItem = results[0];
            var secondItem = results[1];
            var thirdItem = results[2];
            BaseAssertions(results);

            Assert.AreEqual(10, firstItem.Metadata.Count);
            Assert.AreEqual(10, secondItem.Metadata.Count);
            Assert.AreEqual(3, thirdItem.Metadata.Count);


            Assert.AreEqual(1, firstItem.Metadata.MinUid);
            Assert.AreEqual(10, firstItem.Metadata.MaxUid);

            Assert.AreEqual(11, secondItem.Metadata.MinUid);
            Assert.AreEqual(20, secondItem.Metadata.MaxUid);

            Assert.AreEqual(21, thirdItem.Metadata.MinUid);
            Assert.AreEqual(23, thirdItem.Metadata.MaxUid);


        }

        [TestMethod]
        public void GroupByChunkTest_TEST_UPDATES() {

            //already one chunk in place
            var chunks = new List<RedisLookupRowstampChunkHash>{
                new RedisLookupRowstampChunkHash{
                    RealKey = "xxx;chunk:1",
                    Count = 10,
                    MinUid = 1,
                    MaxUid = 10
                }
            };

            var descriptor = new RedisChunkMetadataDescriptor {
                Chunks = chunks
            };
            var datamaps = GenerateListOfDataMaps(15, 0);
            var input = new RedisInputDTO<JSONConvertedDatamap>(datamaps);
            var results = _redisManager.GroupDatamapsByChunk(descriptor, input, 10).Results;
            Assert.AreEqual(2, results.Count);

            var firstItem = results[0];
            var secondItem = results[1];

            BaseAssertions(results);


            Assert.AreEqual(10, firstItem.Metadata.Count);
            Assert.AreEqual(5, secondItem.Metadata.Count);


            Assert.AreEqual(1, firstItem.Metadata.MinUid);
            Assert.AreEqual(10, firstItem.Metadata.MaxUid);

            Assert.AreEqual(11, secondItem.Metadata.MinUid);
            Assert.AreEqual(15, secondItem.Metadata.MaxUid);

        }


        [TestMethod]
        public void GroupByChunkTest_TEST_UPDATES_2_CHUNKS() {

            //already one chunk in place
            var chunks = new List<RedisLookupRowstampChunkHash>{
                new RedisLookupRowstampChunkHash{
                    RealKey = "xxx;chunk:1",
                    Count = 10,
                    MinUid = 1,
                    MaxUid = 10
                },
                new RedisLookupRowstampChunkHash{
                    RealKey = "xxx;chunk:2",
                    Count = 8,
                    MinUid = 11,
                    MaxUid = 18
                }
            };

            var descriptor = new RedisChunkMetadataDescriptor {
                Chunks = chunks
            };
            var datamaps = GenerateListOfDataMaps(30, 0);
            var input = new RedisInputDTO<JSONConvertedDatamap>(datamaps);
            var groupDatamapsResult = _redisManager.GroupDatamapsByChunk(descriptor, input, 10);
            Assert.AreEqual(1L, groupDatamapsResult.MaxRowstamp);
            var results = groupDatamapsResult.Results;
            Assert.AreEqual(3, results.Count);

            var firstItem = results[0];
            var secondItem = results[1];
            var thirdItem = results[2];

            BaseAssertions(results);

            Assert.AreEqual(10, firstItem.Metadata.Count);
            Assert.AreEqual(10, secondItem.Metadata.Count);
            Assert.AreEqual(10, thirdItem.Metadata.Count);





            Assert.AreEqual(1, firstItem.Metadata.MinUid);
            Assert.AreEqual(10, firstItem.Metadata.MaxUid);

            Assert.AreEqual(10, firstItem.ToUpdate.Count);
            Assert.AreEqual(0, firstItem.ToInsert.Count);

            Assert.AreEqual(11, secondItem.Metadata.MinUid);
            Assert.AreEqual(20, secondItem.Metadata.MaxUid);

            Assert.AreEqual(8, secondItem.ToUpdate.Count);
            Assert.AreEqual(2, secondItem.ToInsert.Count);

            Assert.AreEqual(21, thirdItem.Metadata.MinUid);
            Assert.AreEqual(30, thirdItem.Metadata.MaxUid);

            Assert.AreEqual(0, thirdItem.ToUpdate.Count);
            Assert.AreEqual(10, thirdItem.ToInsert.Count);

        }

        private static void BaseAssertions(IList<RedisManager.GrouppedDatamaps<JSONConvertedDatamap>> items) {
            for (var i = 0; i < items.Count; i++) {
                var item = items[i];
                Assert.IsNotNull(item.Metadata);
                Assert.AreEqual("xxx;chunk:" + (i + 1), item.Metadata.RealKey);
            }
        }

        private List<JSONConvertedDatamap> GenerateListOfDataMaps(int count, int offSet = 0) {
            var results = new List<JSONConvertedDatamap>();
            for (var i = 1; i < count + 1; i++) {
                var dict = new Dictionary<string, object>();
                dict["locationuid"] = i + offSet;
                dict["rowstamp"] = 1L;
                var dm = DataMap.GetInstanceFromDictionary("location", dict, true);
                dm.Id = (i + offSet).ToString();
                results.Add(new JSONConvertedDatamap(dm, true));
            }
            return results;
        }
        #endregion

        [TestMethod]
        public async Task TestFirstTimeInsert() {

            var datamaps = GenerateListOfDataMaps(15);
            var input = new RedisInputDTO<JSONConvertedDatamap>(datamaps);


            var descriptor = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 1L,
                Chunks = new List<RedisLookupRowstampChunkHash>{
                    new RedisLookupRowstampChunkHash {RealKey = "xxx;chunk:1"},
                    new RedisLookupRowstampChunkHash {RealKey = "xxx;chunk:2"},
                }
            };

            _cacheClient.Setup(c => c.AddAsync("xxx", descriptor)).ReturnsAsync(true);
            _cacheClient.Setup(c => c.AddAsync("xxx;chunk:1", It.IsAny<List<JSONConvertedDatamap>>())).ReturnsAsync(true);
            _cacheClient.Setup(c => c.AddAsync("xxx;chunk:2", It.IsAny<List<JSONConvertedDatamap>>())).ReturnsAsync(true);

            var firstTimeInsertResult = await _redisManager.FirstTimeInsert(input, "xxx", 10);
            Assert.AreEqual(1L, firstTimeInsertResult.MaxRowstamp);
            var results = firstTimeInsertResult.Results;

            Assert.AreEqual(2, results.Count);
            var firstChunk = results[0];

            Assert.AreEqual("xxx;chunk:1", firstChunk.RealKey);
            Assert.AreEqual(1, firstChunk.MinUid);
            Assert.AreEqual(10, firstChunk.MaxUid);
            Assert.AreEqual(10, firstChunk.Count);

            var secondChunk = results[1];
            Assert.AreEqual("xxx;chunk:2", secondChunk.RealKey);
            Assert.AreEqual(11, secondChunk.MinUid);
            Assert.AreEqual(15, secondChunk.MaxUid);
            Assert.AreEqual(5, secondChunk.Count);

            TestUtil.VerifyMocks(_cacheClient);

        }


        [TestMethod]
        public async Task TestFirstTimeInsertReverseOrder() {

            var datamaps = GenerateListOfDataMaps(15);
            datamaps.Reverse();
            var input = new RedisInputDTO<JSONConvertedDatamap>(datamaps);


            var descriptor = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 1L,
                Chunks = new List<RedisLookupRowstampChunkHash>{
                    new RedisLookupRowstampChunkHash {RealKey = "xxx;chunk:1"},
                    new RedisLookupRowstampChunkHash {RealKey = "xxx;chunk:2"},
                }
            };

            _cacheClient.Setup(c => c.AddAsync("xxx", descriptor)).ReturnsAsync(true);
            _cacheClient.Setup(c => c.AddAsync("xxx;chunk:1", It.IsAny<List<JSONConvertedDatamap>>())).ReturnsAsync(true);
            _cacheClient.Setup(c => c.AddAsync("xxx;chunk:2", It.IsAny<List<JSONConvertedDatamap>>())).ReturnsAsync(true);

            var firstTimeInsertResult = await _redisManager.FirstTimeInsert(input, "xxx", 10);
            Assert.AreEqual(1L, firstTimeInsertResult.MaxRowstamp);
            var results = firstTimeInsertResult.Results;

            Assert.AreEqual(2, results.Count);
            var firstChunk = results[0];

            Assert.AreEqual("xxx;chunk:1", firstChunk.RealKey);
            Assert.AreEqual(1, firstChunk.MinUid);
            Assert.AreEqual(10, firstChunk.MaxUid);
            Assert.AreEqual(10, firstChunk.Count);

            var secondChunk = results[1];
            Assert.AreEqual("xxx;chunk:2", secondChunk.RealKey);
            Assert.AreEqual(11, secondChunk.MinUid);
            Assert.AreEqual(15, secondChunk.MaxUid);
            Assert.AreEqual(5, secondChunk.Count);

            TestUtil.VerifyMocks(_cacheClient);

        }


        [TestMethod]
        public async Task TestLookupBringOnlyThirdChunk() {

            var schema = new ApplicationSchemaDefinition {
                SchemaId = "list",
                ApplicationName = "location"
            };

            var lookupDTO = new RedisLookupDTO {
                Schema = schema,
                GlobalLimit = 10,
                CacheRoundtripStatuses = new Dictionary<string, CacheRoundtripStatus>{ { "application=location;schemaid=list;chunk:1", new CacheRoundtripStatus(){Complete = true}}
                    , {"application=location;schemaid=list;chunk:2", new CacheRoundtripStatus(){Complete = true} }}
            };


            var descriptor = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 100L
            };

            var list = GenerateListOfDataMaps(10);

            descriptor.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;schemaid=list;chunk:1", Count = 10 });
            descriptor.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;schemaid=list;chunk:2", Count = 10 });
            descriptor.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;schemaid=list;chunk:3", Count = 10 });
            descriptor.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;schemaid=list;chunk:4", Count = 8 });

            _cacheClient.Setup(c => c.GetAsync<RedisChunkMetadataDescriptor>("application=location;schemaid=list")).ReturnsAsync(descriptor);
            _cacheClient.Setup(c => c.GetAsync<IList<JSONConvertedDatamap>>("application=location;schemaid=list;chunk:3")).ReturnsAsync(list);
            //            _cacheClient.Setup(c => c.GetAsync<IList<DataMap>>("application=location;schemaid=list;chunk:4")).ReturnsAsync(list);

            var results = await _redisManager.Lookup<JSONConvertedDatamap>(lookupDTO);
            Assert.AreEqual(1, results.Chunks.Count);
            Assert.AreEqual(2, results.ChunksAlreadyChecked.Count);
            Assert.AreEqual(1, results.ChunksIgnored.Count);
            Assert.AreEqual(100L, descriptor.MaxRowstamp);

            Assert.IsTrue(results.HasMoreChunks);

            Assert.AreEqual(list, results.Chunks[0].Results);
            Assert.AreEqual(10, results.Chunks[0].Results.Count);

            TestUtil.VerifyMocks(_cacheClient);
        }

        [TestMethod]
        public async Task TestLookupBringiningOnlyFirstChunk() {

            var schema = new ApplicationSchemaDefinition {
                SchemaId = "list",
                ApplicationName = "location"
            };

            var lookupDTO = new RedisLookupDTO {
                Schema = schema,
                GlobalLimit = 10,
            };


            var descriptor = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 100L
            };

            var list = GenerateListOfDataMaps(10);

            descriptor.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;schemaid=list;chunk:1", Count = 10 });
            descriptor.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;schemaid=list;chunk:2", Count = 10 });
            descriptor.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;schemaid=list;chunk:3", Count = 10 });
            descriptor.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;schemaid=list;chunk:4", Count = 8 });

            _cacheClient.Setup(c => c.GetAsync<RedisChunkMetadataDescriptor>("application=location;schemaid=list")).ReturnsAsync(descriptor);
            _cacheClient.Setup(c => c.GetAsync<IList<JSONConvertedDatamap>>("application=location;schemaid=list;chunk:1")).ReturnsAsync(list);

            var results = await _redisManager.Lookup<JSONConvertedDatamap>(lookupDTO);
            Assert.AreEqual(1, results.Chunks.Count);
            Assert.AreEqual(0, results.ChunksAlreadyChecked.Count);
            Assert.AreEqual(3, results.ChunksIgnored.Count);
            Assert.AreEqual(100L, descriptor.MaxRowstamp);
            Assert.AreEqual(list, results.Chunks[0].Results);
            Assert.AreEqual(10, results.Chunks[0].Results.Count);
            Assert.IsTrue(results.HasMoreChunks);

            TestUtil.VerifyMocks(_cacheClient);
        }


        [TestMethod]
        public async Task TestMultipleKeysFoundFirstBringOnlyExact() {

            var schema = new ApplicationSchemaDefinition {
                SchemaId = "list",
                ApplicationName = "location"
            };

            var facilities = new List<string> { "aes", "acs", "tpz" };

            var lookupDTO = new RedisLookupDTO {
                Schema = schema,
                GlobalLimit = 100000,
                ExtraKeys = new Dictionary<string, object> { { "facilities", facilities } }
            };


            var descriptor1 = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 100L
            };

            var descriptor2 = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 100L
            };

            var descriptor3 = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 100L
            };

            var descriptor4 = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 100L
            };

            var list = GenerateListOfDataMaps(10);

            descriptor1.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;facilities=acs,aes,tpz;schemaid=list;chunk:1", Count = 10 });
            descriptor2.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;facilities=acs;schemaid=list;chunk:1", Count = 10 });
            descriptor3.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;facilities=aes;schemaid=list;chunk:1", Count = 10 });
            descriptor4.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;facilities=tpz;schemaid=list;chunk:1", Count = 10 });


            _cacheClient.Setup(c => c.GetAsync<RedisChunkMetadataDescriptor>("application=location;facilities=acs,aes,tpz;schemaid=list")).ReturnsAsync(descriptor1);
            _cacheClient.Setup(c => c.GetAsync<IList<JSONConvertedDatamap>>("application=location;facilities=acs,aes,tpz;schemaid=list;chunk:1")).ReturnsAsync(list);

            var results = await _redisManager.Lookup<JSONConvertedDatamap>(lookupDTO);
            Assert.AreEqual(1, results.Chunks.Count);
            Assert.AreEqual(0, results.ChunksAlreadyChecked.Count);
            Assert.AreEqual(0, results.ChunksIgnored.Count);
            Assert.AreEqual(100L, descriptor1.MaxRowstamp);
            Assert.AreEqual(list, results.Chunks[0].Results);
            Assert.AreEqual(10, results.Chunks[0].Results.Count);
            Assert.IsFalse(results.HasMoreChunks);


            TestUtil.VerifyMocks(_cacheClient);
        }


        [TestMethod]
        public async Task TestMultipleKeysFirstNotFoundBringOthers() {

            var schema = new ApplicationSchemaDefinition {
                SchemaId = "list",
                ApplicationName = "location"
            };

            var facilities = new List<string> { "aes", "acs" };

            var lookupDTO = new RedisLookupDTO {
                Schema = schema,
                GlobalLimit = 100000,
                ExtraKeys = new Dictionary<string, object> { { "facilities", facilities } }
            };


            var descriptor1 = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 100L
            };

            var descriptor2 = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 100L
            };

            var descriptor3 = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 100L
            };

            var descriptor4 = new RedisChunkMetadataDescriptor {
                MaxRowstamp = 100L
            };

            var list = GenerateListOfDataMaps(10);

//            descriptor1.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;facilities=acs,aes;schemaid=list;chunk:1", Count = 10 });
            descriptor2.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;facilities=acs;schemaid=list;chunk:1", Count = 10 });
            descriptor3.Chunks.Add(new RedisLookupRowstampChunkHash { RealKey = "application=location;facilities=aes;schemaid=list;chunk:1", Count = 10 });


            _cacheClient.Setup(c => c.GetAsync<RedisChunkMetadataDescriptor>("application=location;facilities=acs,aes;schemaid=list")).ReturnsAsync(null);
            _cacheClient.Setup(c => c.GetAsync<RedisChunkMetadataDescriptor>("application=location;facilities=acs;schemaid=list")).ReturnsAsync(descriptor2);
            _cacheClient.Setup(c => c.GetAsync<RedisChunkMetadataDescriptor>("application=location;facilities=aes;schemaid=list")).ReturnsAsync(descriptor3);

            _cacheClient.Setup(c => c.GetAsync<IList<JSONConvertedDatamap>>("application=location;facilities=acs;schemaid=list;chunk:1")).ReturnsAsync(list);
            _cacheClient.Setup(c => c.GetAsync<IList<JSONConvertedDatamap>>("application=location;facilities=aes;schemaid=list;chunk:1")).ReturnsAsync(list);



            var results = await _redisManager.Lookup<JSONConvertedDatamap>(lookupDTO);
            Assert.AreEqual(2, results.Chunks.Count);
            Assert.AreEqual(0, results.ChunksAlreadyChecked.Count);
            Assert.AreEqual(0, results.ChunksIgnored.Count);
            Assert.AreEqual(100L, descriptor1.MaxRowstamp);
            Assert.AreEqual(list, results.Chunks[0].Results);
            Assert.AreEqual(10, results.Chunks[0].Results.Count);
            Assert.IsFalse(results.HasMoreChunks);


            TestUtil.VerifyMocks(_cacheClient);
        }


    }
}
