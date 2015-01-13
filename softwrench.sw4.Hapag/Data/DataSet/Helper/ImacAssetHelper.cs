using softWrench.sW4.Util;
using System.Collections.Generic;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {

    class ImacAssetHelper {


        /// <summary>
        /// gets the status of the assets that would need to be filtered for each scenario of the imacs.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="childAsset"></param>
        /// <param name="isNew"></param>
        /// <returns></returns>
        public static string[] GetImacStatusToSearch(string schema, bool childAsset, bool isNew) {
            if (schema.EqualsIc(ImacConstants.ReplaceStd) && !isNew) {
                //fix for https://controltechnologysolutions.atlassian.net/browse/HAP-683
                return new[] { AssetConstants.Operating, AssetConstants.Active };
            }
            if (schema.StartsWith(ImacConstants.Install) || (schema.StartsWith(ImacConstants.Replace) && isNew)) {
                //on replace the new assets should behave like the install ones, and the "old" as the remove ones
                return new[] { AssetConstants.Ordered, AssetConstants.Idle };
            }
            return new[] { AssetConstants.Operating, AssetConstants.Active, AssetConstants.Idle };
        }



        public static SortedSet<string> GetImacOptionsFromStatus(string assetStatus) {
            var toFilter = new SortedSet<string>();
            
            if (!assetStatus.EqualsAny(AssetConstants.Ordered, AssetConstants.Idle)) {
                toFilter.Add(ImacConstants.Install);
            }
            if (!assetStatus.EqualsAny(AssetConstants.Operating, AssetConstants.Active, AssetConstants.Idle)) {
                toFilter.Add(ImacConstants.Update);
                toFilter.Add(ImacConstants.Remove);
                toFilter.Add(ImacConstants.Move);
                toFilter.Add(ImacConstants.Replace);
                toFilter.Add(ImacConstants.Add);
            }
            return toFilter;
        }

        public static SortedSet<string> GetImacOptionsFromClassStructure(string classstructure) {
            var toFilter = new SortedSet<string>();
            if (!AssetConstants.Addclassifications.Contains("'" + classstructure + "'")) {
                toFilter.Add(ImacConstants.Add);
            }
            return toFilter;
        }


    }
}
