using System;
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

            var newResult = new Dictionary<string, List<DataMap>>();

            var first = true;
            var associationDatasKeys = new SortedSet<string>(results.AssociationData.Keys);

            var toKeepDict = new Dictionary<string, int?>();

            foreach (var associationDataKey in associationDatasKeys) {
                sum += results.AssociationData[associationDataKey].Count;
                if (sum >= maxSize && !first) {
                    toKeepDict[associationDataKey] = sum - maxSize;
                    results.HasMoreData = true;
                    break;
                }
                toKeepDict[associationDataKey] = null;
                if (sum >= maxSize) {
                    //scenario where the first item is already bigger than the limit
                    results.HasMoreData = true;
                    break;
                }

                first = false;
            }
            //avoid copying huge datasets by excluding the ones that should not be downloaded instead
            foreach (var associationDataKey in associationDatasKeys) {
                if (!toKeepDict.ContainsKey(associationDataKey)) {
                    results.AssociationData.Remove(associationDataKey);
                } else if (toKeepDict[associationDataKey] != null) {
                    results.AssociationData[associationDataKey] = results.AssociationData[associationDataKey].Take(toKeepDict[associationDataKey].Value).ToList();
                }
            }

            return results;

        }
    }
}
