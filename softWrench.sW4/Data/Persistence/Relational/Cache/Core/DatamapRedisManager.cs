﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using cts.commons.simpleinjector.Events;
using log4net;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data.Configuration;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;

namespace softWrench.sW4.Data.Persistence.Relational.Cache.Core {

    public class DatamapRedisManager : BaseRedisManager, IDatamapRedisManager {

        private readonly ILog _log = LogManager.GetLogger(typeof(DatamapRedisManager));

        public DatamapRedisManager(IConfigurationFacade configFacade) : base(configFacade) {
        }



        public async Task<List<RedisChunkMetadataDescriptor>> GetDescriptors(RedisLookupDTO lookupDTO) {
            var descriptors = new List<RedisChunkMetadataDescriptor>();

            if (CacheClient == null) {
                _log.WarnFormat("cache is disabled. Skipping get descriptors operation");
                return descriptors;
            }

            //multiple keys, one for each facility for instance
            var keys = lookupDTO.BuildKeys();
            foreach (var key in keys) {
                var metadataDescriptor = await CacheClient.GetAsync<RedisChunkMetadataDescriptor>(key);
                if (metadataDescriptor == null) {
                    //especially the first descriptor is the linear combination for multiple keys scenario (ex a user with more than one facility)
                    //therefore it might be null quite often.
                    // other keys will be the linear combination for these scenarios.
                    _log.DebugFormat("descriptor for entry {0} not found", key);
                    continue;
                }
                descriptors.Add(metadataDescriptor);
            }
            return descriptors;
        }

        public async Task<RedisLookupResult<T>> Lookup<T>(RedisLookupDTO lookupDTO) where T : DataMap {
            if (CacheClient == null) {
                _log.WarnFormat("cache is disabled. Skipping lookup operation");
                return new RedisLookupResult<T> {
                    Schema = lookupDTO.Schema
                };
            }
            //multiple keys, one for each facility for instance
            var keys = lookupDTO.BuildKeys();
            var results = new List<RedisResultDTO<T>>();

            var result = new RedisLookupResult<T> {
                Chunks = results,
                Schema = lookupDTO.Schema,
                ChunksAlreadyChecked = lookupDTO.CacheRoundtripStatuses,
            };

            var cacheLimit = 0;


            //            var hasMultipleKeys = keys.Count > 1;

            var first = true;
            //usually just one key will be present

            foreach (var key in keys) {

                if (cacheLimit > lookupDTO.GlobalLimit) {
                    _log.DebugFormat("global limit {0} of the chunk has been reached. pointless to continue ", lookupDTO.GlobalLimit);
                    break;
                }

                _log.DebugFormat("looking for cache entry {0}", key);

                if (lookupDTO.CacheRoundtripStatuses.ContainsKey(key) && lookupDTO.CacheRoundtripStatuses[key].Complete) {
                    //scenario where the cache is refreshed while the roundtrip download is being performed
                    _log.DebugFormat("ignoring the whole descriptor since the cache is just being populated for this given roundtrip", key);
                    continue;
                }

                //list of chunks of cached data
                var metadataDescriptor = await CacheClient.GetAsync<RedisChunkMetadataDescriptor>(key);


                if (metadataDescriptor == null) {
                    //especially the first descriptor is the linear combination for multiple keys scenario (ex a user with more than one facility)
                    //therefore it might be null quite often.
                    // other keys will be the linear combination for these scenarios.
                    _log.DebugFormat("cache entry {0} not found", key);
                    result.NotFoundDescriptors.Add(key);
                    first = false;
                    continue;
                }

                result.MaxRowstamp = metadataDescriptor.MaxRowstamp;
                var resultChunksDescriptors = metadataDescriptor.Chunks;

                if (resultChunksDescriptors.Count == 1) {
                    _log.DebugFormat("single chunk of data found for cache entry {0}", key);
                }


                foreach (var entry in resultChunksDescriptors) {
                    if (cacheLimit >= lookupDTO.GlobalLimit) {
                        result.HasMoreChunks = true;
                        _log.DebugFormat("global limit {0} of the chunk has been reached. ignoring chunk {1} ", lookupDTO.GlobalLimit, entry.RealKey);
                        result.ChunksIgnored.Add(entry.RealKey);
                        continue;
                    }

                    if (lookupDTO.CacheRoundtripStatuses.ContainsKey(entry.RealKey) && lookupDTO.CacheRoundtripStatuses[entry.RealKey].Complete) {
                        _log.DebugFormat("ignoring cache entry {0} cause it was already handled", entry.RealKey);
                        continue;
                    }

                    //                    if (lookupDTO.MaxUid.HasValue && (lookupDTO.MaxUid >= entry.MaxUid)) {
                    //                        continue;
                    //                    }

                    var cacheResult = await CacheClient.GetAsync<IList<T>>(entry.RealKey);
                    var count = entry.Count;
                    if (lookupDTO.CacheRoundtripStatuses.ContainsKey(entry.RealKey)) {
                        var offSetPosition = lookupDTO.CacheRoundtripStatuses[entry.RealKey].Position;
                        _log.DebugFormat("skipping {0} entries from cache {1} since they were already downloaded at another roundtrup", offSetPosition, key);
                        cacheResult = new List<T>(cacheResult.Skip(offSetPosition));
                    }

                    results.Add(new RedisResultDTO<T>(
                        entry.RealKey, cacheResult
                    ));
                    cacheLimit += count;
                }

                if (first) {
                    //if multiple keys were found, the first one will be the linear combination of them all.
                    //thus, returning it (case it is found) should be enough
                    break;
                }

            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lookupDTO"></param>
        /// <param name="redisInput"></param>
        /// <returns>The maxrowstamp amongst all entries that were added so that we have a refernce to get from the database nexttime</returns>
        public async Task<long> InsertIntoCache<T>(RedisLookupDTO lookupDTO, RedisInputDTO<T> redisInput) where T : DataMap {
            if (!ServiceAvailable) {
                _log.WarnFormat("cache is disabled. Skipping insert operation");
                return 0L;
            }

            //multiple keys, one for each facility for instance
            var keys = lookupDTO.BuildKeys();
            //first key would be the combination of multiple extrakeys in a scenario where these are provided (ex: multiple facilities--> store all of the facilities hash)
            //we should not store other keys (which would be individual entrie combinations), while the results informed cannot refer to multiple keys, but rather just a single one.
            var firstKey = keys.First();

            var resultChunksDescriptors = await CacheClient.GetAsync<RedisChunkMetadataDescriptor>(firstKey);
            if (resultChunksDescriptors == null) {
                var results = await FirstTimeInsert(redisInput, firstKey, lookupDTO.GlobalLimit);
                return results.MaxRowstamp;
            }

            var collectionsByChunkResult = GroupDatamapsByChunk(resultChunksDescriptors, redisInput, lookupDTO.GlobalLimit);

            var collectionByChunk = collectionsByChunkResult.Results;

            //one thread per cache chunk that needs to be updated
            var tasks = new Task[collectionByChunk.Count(c => c.AnyOperation)];

            var i = 0;
            foreach (var entry in collectionByChunk) {
                tasks[i++] = DoUpdateChunk(entry);
            }

            await UpdateMetadataDescriptor(resultChunksDescriptors, firstKey, collectionsByChunkResult.MaxRowstamp);

            return collectionsByChunkResult.MaxRowstamp;
        }

        private async Task UpdateMetadataDescriptor(RedisChunkMetadataDescriptor resultChunksDescriptors, string firstKey, long maxRowstamp) {
            //at least one new chunk has been generated [ex: we had 100k entries already (2 chunks), and added another 110k items (3 new chunks)]
            //need to update the root descriptor
            resultChunksDescriptors.Chunks = resultChunksDescriptors.Chunks;
            resultChunksDescriptors.MaxRowstamp = maxRowstamp;
            await CacheClient.ReplaceAsync(firstKey, resultChunksDescriptors);
        }

        private async Task DoUpdateChunk<T>(GrouppedDatamaps<T> grouppedDatamaps) where T : DataMap {
            var toUpdate = grouppedDatamaps.ToUpdate;
            var toInsert = grouppedDatamaps.ToInsert;

            //items that should have been on this chunk, but for some reason are not. 
            //For instance, a whereclause might have changed overtime, or a status of an asset, which caused it to be included later
            //For these scenarios, the chunk size might become bigger than the globallimit, cause otherwise we´d need to reindex all of the chunks othersiwe to garantee it
            var toInsertOverflown = new List<T>();

            var descriptor = grouppedDatamaps.Metadata;

            var watch = Stopwatch.StartNew();

            _log.InfoFormat("updating chunk entry {0} with {1} entries to update and {2} to insert", descriptor, toUpdate.Count, toInsert.Count);
            var items = await CacheClient.GetAsync<List<T>>(grouppedDatamaps.Metadata.RealKey);


            items.AddRange(toInsert);


            foreach (var newDatamap in toUpdate) {
                var idx = items.FindIndex(a => GetId(a) == GetId(newDatamap));
                if (idx != -1) {
                    //TODO: improve algorithm to check for secondary rowstamps, such in projection values 
                    //(ex: an asset didn´t change but one of the relationships where a projection fetchs data did, meaning that, therefore the result is different)
                    if (newDatamap.Approwstamp != items[idx].Approwstamp) {
                        //double checking the whether item has really changed
                        items[idx] = newDatamap;
                    }


                } else {
                    //item not found on the preferred chunk
                    toInsertOverflown.Add(newDatamap);
                }

            }

            //TODO: double check sort order.
            var maxUId = GetId(items[items.Count - 1]);

            //eventual items that were not present on the original chunk, although were within the chunk ranges
            items.AddRange(toInsertOverflown);

            await CacheClient.ReplaceAsync(grouppedDatamaps.Metadata.RealKey, items);

            descriptor.MaxUid = maxUId;

            descriptor.Count = items.Count;

            _log.DebugFormat("chunk update for {0}  took {1}", grouppedDatamaps.Metadata.RealKey, watch.ElapsedMilliseconds);
            watch.Stop();
        }

        internal class GrouppedDatamaps<T> where T : DataMap {
            internal IList<T> ToInsert { get; } = new List<T>();
            internal IList<T> ToUpdate { get; } = new List<T>();

            internal RedisLookupRowstampChunkHash Metadata { get; set; }

            public bool AnyOperation => ToInsert.Any() || ToUpdate.Any();
        }

        internal class GroupDatamapsResult<T> where T : DataMap {

            internal long MaxRowstamp;
            internal IList<GrouppedDatamaps<T>> Results;

            public GroupDatamapsResult(long maxRowstamp, IList<GrouppedDatamaps<T>> results) {
                MaxRowstamp = maxRowstamp;
                Results = results;
            }
        }

        internal virtual GroupDatamapsResult<T> GroupDatamapsByChunk<T>(RedisChunkMetadataDescriptor resultChunksDescriptors, RedisInputDTO<T> redisInput, int globalLimit) where T : DataMap {

            var results = new Dictionary<RedisLookupRowstampChunkHash, GrouppedDatamaps<T>>();


            var bigggestUid = 0L;
            foreach (var descriptor in resultChunksDescriptors.Chunks) {
                results[descriptor] = new GrouppedDatamaps<T> { Metadata = descriptor };
                bigggestUid = Math.Max(bigggestUid, descriptor.MaxUid);
            }

            //for instance --> chunks of 50k and there were only 20k on the last chunk --> 30k to add there
            var lastChunkSpace = globalLimit - resultChunksDescriptors.Chunks.Last().Count;

            var lastChunk = results.Keys.Last();

            long maxRowstamp = 0L;

            foreach (var datamap in redisInput.Datamaps) {
                var uid = GetId(datamap);
                maxRowstamp = Math.Max(datamap.Approwstamp.Value, maxRowstamp);

                if (uid <= bigggestUid) {
                    //updating an existing one
                    var found = false;
                    foreach (var descriptor in resultChunksDescriptors.Chunks) {
                        if (uid >= descriptor.MinUid && uid <= descriptor.MaxUid) {
                            //only a possibility, can´t tell for sure, cause there can be gaps
                            // in that case, it needs to be inserted on the last chunk on a second pass later
                            results[descriptor].ToUpdate.Add(datamap);
                            found = true;
                        }
                        if (found) {
                            break;
                        }
                    }
                    if (found) {
                        continue;
                    }
                }

                //either the uid of the item was bigger than all of the chunks by design, or for some unexpected behaviour, we´d add it to the end anyways

                _log.DebugFormat("adding new item for insertion at last possible chunk");
                if (lastChunkSpace <= 0) {
                    DoUpdateLastChunkDescriptor(results, lastChunk);

                    //negative space accounts for an anomalous scenario whereas the chunk limit got updated and the caches haven´t been updated 
                    _log.DebugFormat("generating a new chunk of data to store new items");
                    var newDescriptor = new RedisLookupRowstampChunkHash();
                    newDescriptor.CopyKeyIncrementingChunkNumber(lastChunk, results.Count + 1);
                    lastChunkSpace = globalLimit;
                    lastChunk = newDescriptor;
                    if (results.ContainsKey(lastChunk)) {
                        results[lastChunk] = new GrouppedDatamaps<T> { Metadata = newDescriptor };
                    } else {
                        results.Add(lastChunk, new GrouppedDatamaps<T> { Metadata = newDescriptor });
                    }

                }

                results[lastChunk].ToInsert.Add(datamap);
                lastChunkSpace--;

            }
            DoUpdateLastChunkDescriptor(results, lastChunk);



            return new GroupDatamapsResult<T>(maxRowstamp, results.Values.ToList());
        }

        private static void DoUpdateLastChunkDescriptor<T>(IReadOnlyDictionary<RedisLookupRowstampChunkHash, GrouppedDatamaps<T>> results,
            RedisLookupRowstampChunkHash lastChunk) where T : DataMap {
            var toInsertList = results[lastChunk].ToInsert;
            if (toInsertList.Any()) {
                lastChunk.Count += toInsertList.Count;
                if (lastChunk.MinUid == 0) {
                    lastChunk.MinUid = GetId(toInsertList.First());
                }
                lastChunk.MaxUid = GetId(toInsertList.Last());
            }
        }

        private static long GetId<T>(T entry) where T : DataMap {
            return entry.GetLongAttribute("Id").Value;
        }

        internal class FirstTimeInsertResult {

            internal readonly IList<RedisLookupRowstampChunkHash> Results;
            internal readonly long MaxRowstamp;

            public FirstTimeInsertResult(IList<RedisLookupRowstampChunkHash> results, long maxRowstamp) {
                Results = results;
                MaxRowstamp = maxRowstamp;
            }


        }


        internal async Task<FirstTimeInsertResult> FirstTimeInsert<T>(RedisInputDTO<T> redisInput, string firstKey, int globalLimit) where T : DataMap {
            _log.InfoFormat("Creating cache entries for {0}", firstKey);

            IList<RedisLookupRowstampChunkHash> resultChunksDescriptors = new List<RedisLookupRowstampChunkHash>();

            var tempList = new List<T>();

            var chunkNumber = 0;
            var minUid = 0L;

            var maxRowstamp = 0L;
            var redisInputDatamaps = redisInput.Datamaps;

            if (!redisInputDatamaps.Any()) {
                return new FirstTimeInsertResult(resultChunksDescriptors, maxRowstamp);
            }


            if (redisInputDatamaps.Count > 1) {
                var firstId = long.Parse(redisInputDatamaps[0].Id);
                var secondId = long.Parse(redisInputDatamaps[1].Id);
                if (firstId > secondId) {
                    //list was passed on the opposite order
                    redisInputDatamaps = redisInputDatamaps.Reverse().ToList();
                }
            }




            var i = 0;

            foreach (var data in redisInputDatamaps) {
                tempList.Add(data);
                maxRowstamp = Math.Max(maxRowstamp, data.Approwstamp.Value);

                if (tempList.Count == 1) {
                    //items should be sorted in ascending uid order
                    minUid = Convert.ToInt64(data.Id);
                }

                i++;


                if (tempList.Count % globalLimit == 0 || i == redisInputDatamaps.Count()) {
                    chunkNumber++;

                    var maxUid = Convert.ToInt64(data.Id);

                    var realKey = firstKey + ";chunk:" + chunkNumber;

                    resultChunksDescriptors.Add(new RedisLookupRowstampChunkHash {
                        Count = tempList.Count,
                        RealKey = realKey,
                        MinUid = minUid,
                        MaxUid = maxUid
                    });

                    await CacheClient.AddAsync(realKey, tempList);

                    tempList.Clear();
                }
            }

            await CacheClient.AddAsync(firstKey, new RedisChunkMetadataDescriptor {
                Chunks = resultChunksDescriptors,
                MaxRowstamp = maxRowstamp
            });

            return new FirstTimeInsertResult(resultChunksDescriptors, maxRowstamp);
        }

        


    }
}