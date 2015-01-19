using softWrench.sW4.Util;
using System.Collections.Generic;

namespace softwrench.sw4.Hapag.Data.DataSet.Helper {

    class ImacAssetHelper {


        /// <summary>
        /// gets the status of the assets that would need to be filtered for each scenario of the imacs.
        /// 
        /// check https://controltechnologysolutions.atlassian.net/browse/HAP-827 attached doc for details
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="childAsset"></param>
        /// <param name="isNew"></param>
        /// <returns></returns>
        public static string[] GetImacStatusToSearch(string schema, bool childAsset, bool isNew) {
            if (schema.EqualsIc(ImacConstants.Add)) {
                return childAsset ? new[] { AssetConstants.Idle } : new[] { AssetConstants.Operating, AssetConstants.Active };
            }

            if ((schema.StartsWith(ImacConstants.Replace) && !isNew) || (schema.StartsWith(ImacConstants.Remove))) {
                if (childAsset) {
                    //replace child assets should be only the associated child assets
                    return null;
                }
                return new[] { AssetConstants.Operating, AssetConstants.Active };
            }
            if ((schema.StartsWith(ImacConstants.Replace) && isNew) || schema.StartsWith(ImacConstants.Install)) {
                //on replace the new assets should behave like the install ones, and the "old" as the remove ones
                if (schema.Equals(ImacConstants.InstallLan) && childAsset) {
                    //implementing Thomas´s request: there should be no child asset selectable but field should be kept (just don’t display anything)
                    return new[] { AssetConstants.Unexistent };
                }

                return new[] { AssetConstants.Ordered, AssetConstants.Idle };
            }

            //TODO: should we apply these conditions for associated child assets?
            return new[] { AssetConstants.Operating, AssetConstants.Active, AssetConstants.Idle };
        }



        public static SortedSet<string> GetImacOptionsFromStatus(string assetStatus) {
            var toFilter = new SortedSet<string>();

            if (!assetStatus.EqualsAny(AssetConstants.Operating, AssetConstants.Active)) {
                toFilter.Add(ImacConstants.Add);
                toFilter.Add(ImacConstants.Replace);
                toFilter.Add(ImacConstants.Remove);
            }

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
