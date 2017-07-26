﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cts.commons.simpleinjector;
using softwrench.sw4.offlineserver.dto.association;
using softwrench.sw4.offlineserver.services.util;
using softWrench.sW4.Configuration.Services.Api;
using softWrench.sW4.Data;

namespace softwrench.sw4.offlineserver.services {

    public class SyncChunkHandler : ISingletonComponent {

        readonly IConfigurationFacade _configurationFacade;

        public SyncChunkHandler(IConfigurationFacade configurationFacade) {
            _configurationFacade = configurationFacade;
        }

        public virtual async Task<AssociationSynchronizationResultDto> HandleMaxSize(AssociationSynchronizationResultDto results) {
            var maxSize = await _configurationFacade.LookupAsync<int>(OfflineConstants.MaxDownloadSize);
            var sum = 0;



            var associationDatasKeys = new SortedSet<string>(results.AssociationData.Keys);

            var toKeepDict = new Dictionary<string, int?>();

            foreach (var associationDataKey in associationDatasKeys) {
                var count = results.AssociationData[associationDataKey].Count;
                if (count == 0) {
                    toKeepDict[associationDataKey] = null;
                    continue;
                }

                if (sum >= maxSize) {
                    results.HasMoreData = true;
                    continue;
                }

                if (sum + count > maxSize) {
                    toKeepDict[associationDataKey] = maxSize - sum;
                    results.HasMoreData = true;
                } else {
                    toKeepDict[associationDataKey] = null;
                }
                sum += count;
            }
            //avoid copying huge datasets by excluding the ones that should not be downloaded instead
            foreach (var associationDataKey in associationDatasKeys) {
                if (!toKeepDict.ContainsKey(associationDataKey)) {
                    //this entry will be discarded completely
                    results.AssociationData[associationDataKey].Clear();
                    results.AssociationData[associationDataKey].Incomplete = true;
                } else if (toKeepDict[associationDataKey] != null) {
                    var associationResult = results.AssociationData[associationDataKey];
                    results.AssociationData[associationDataKey].Incomplete = true;
                    if (results.LimitedAssociations.ContainsKey(associationDataKey) && results.LimitedAssociations[associationDataKey]) {
                        var toSkip = Math.Max(0, associationResult.Count - toKeepDict[associationDataKey].Value);
                        associationResult.Skip(toSkip);
                    } else {
                        associationResult.Take(toKeepDict[associationDataKey].Value);
                    }
                } 
            }

            return results;

        }
    }
}