using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using log4net;
using softwrench.sw4.offlineserver.model.dto.association;
using softwrench.sw4.offlineserver.services.util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;
using softWrench.sW4.Data.Persistence.Relational.Cache.Api;

namespace softwrench.sw4.offlineserver.services {

    public class SyncChunkHandler : ISingletonComponent {

        readonly IConfigurationFacade _configurationFacade;

        private ILog Log = LogManager.GetLogger(typeof(SyncChunkHandler));

        public SyncChunkHandler(IConfigurationFacade configurationFacade) {
            _configurationFacade = configurationFacade;
        }

        public virtual async Task<AssociationSynchronizationResultDto> HandleMaxSize(AssociationSynchronizationResultDto results, bool initialLoad) {
            var maxSize = await _configurationFacade.LookupAsync<int>(OfflineConstants.MaxDownloadSize);
            var sum = 0;



            var associationDatasKeys = new SortedSet<string>(results.AssociationData.Keys);

            var toKeepDict = new Dictionary<string, int?>();

            foreach (var associationDataKey in associationDatasKeys) {
                var count = results.AssociationData[associationDataKey].Count;
                if (count == 0) {
                    //not cutting down
                    toKeepDict[associationDataKey] = null;
                    continue;
                }

                if (sum >= maxSize) {
                    //we´ll need to cut down some chunks --> after the breakpoint others won´t even be added
                    results.HasMoreData = true;
                    continue;
                }

                if (sum + count > maxSize) {
                    //this chunk will be cut down
                    toKeepDict[associationDataKey] = maxSize - sum;
                    results.HasMoreData = true;
                } else {
                    //not cutting down
                    toKeepDict[associationDataKey] = null;
                }
                sum += count;
            }

            //avoid copying huge datasets by excluding the ones that should not be downloaded instead
            foreach (var associationDataKey in associationDatasKeys) {
                var associationDataDto = results.AssociationData[associationDataKey];

                if (!toKeepDict.ContainsKey(associationDataKey)) {
                    //this entry will be discarded completely
                    associationDataDto.Clear();
                    associationDataDto.Incomplete = true;
                } else if (toKeepDict[associationDataKey] != null) {
                    var offSetToKeep = toKeepDict[associationDataKey].Value;
                    HandleCompleteCacheEntries(associationDataDto.CompleteCacheEntries, offSetToKeep);
                    var associationResult = associationDataDto;
                    associationDataDto.Incomplete = true;

                    if (results.LimitedAssociations.ContainsKey(associationDataKey) &&
                        results.LimitedAssociations[associationDataKey]) {
                        var toSkip = Math.Max(0, associationResult.Count - offSetToKeep);
                        associationResult.Skip(toSkip);
                    } else {
                        associationResult.Take(offSetToKeep);
                    }
                } else {
                    //complete entries
                    foreach (var cacheEntry in associationDataDto.CompleteCacheEntries) {
                        cacheEntry.Value.Position += cacheEntry.Value.TransientPosition;
                        cacheEntry.Value.Complete = true;
                    }


                }
            }

            return results;

        }

        private void HandleCompleteCacheEntries(IDictionary<string, CacheRoundtripStatus> completeCacheEntries, int offSetToKeep) {
            var currentLimit = offSetToKeep;
            foreach (var cacheRoundTripEntry in completeCacheEntries) {
                var cacheRoundTrip = cacheRoundTripEntry.Value;
                if (cacheRoundTrip.Complete) {
                    Log.DebugFormat("ignoring cache chunk, since it is already complete");
                    continue;
                }

                if (currentLimit >= 0) {
                    if (cacheRoundTrip.TransientPosition < currentLimit) {
                        Log.DebugFormat(
                            "Cache {0} will be able to be marked as complete, since it has added only {1} out of allowed {2}",
                            cacheRoundTripEntry.Key, cacheRoundTrip.TransientPosition, currentLimit);
                        currentLimit = currentLimit - cacheRoundTrip.TransientPosition;
                        cacheRoundTrip.Complete = true;
                        cacheRoundTrip.Position += cacheRoundTrip.TransientPosition;
                        cacheRoundTrip.TransientPosition = 0;
                    } else {
                        Log.DebugFormat("Applying limit {1} to cache {0}. {2} will be ignored", cacheRoundTripEntry.Key, cacheRoundTrip.TransientPosition, currentLimit);
                        cacheRoundTrip.TransientPosition = 0;
                        cacheRoundTrip.Position += currentLimit;
                        cacheRoundTrip.Complete = false;
                        currentLimit = 0;
                    }
                } else {
                    cacheRoundTrip.Complete = false;
                    cacheRoundTrip.TransientPosition = 0;
                }

            }
        }
    }
}
