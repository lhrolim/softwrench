using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Loader.Custom.Sql;
using softwrench.sw4.Hapag.Data.Sync;
using softWrench.sW4.Security.Entities;
using softwrench.sw4.Shared2.Data.Association;
using softwrench.sw4.Shared2.Util;

namespace softwrench.sw4.Hapag.Security {

    public class HlagLocationUtil {
        private const int LocationIdIdx = 2;
        private const int CostCenterIdx = 3;

        public static Boolean IsSuperGroup(PersonGroup group) {
            return IsSuperGroup(group.Name);
        }

        public static Boolean IsSuperGroup(string subcustomerName) {
            //region groups, doesn´t have the subcustomer id on the description:
            //AMERICA, AMERICA-CANADA vs AMERICA-CANADA-MT2-0040/387400/3870
            return subcustomerName.StartsWith(HapagPersonGroupConstants.BaseHapagRegionPrefix) ||
                   subcustomerName.StartsWith(HapagPersonGroupConstants.BaseHapagAreaPrefix);
        }

        public static bool IsEndUser(PersonGroup group) {
            return group.Name.StartsWith(HapagPersonGroupConstants.HEu);
        }

        public static bool IsExtUser(PersonGroup group) {
            return group.Name.StartsWith(HapagPersonGroupConstants.HExternalUser);
        }

        public static bool IsAProfileGroup(PersonGroup group) {
            return group.Name.StartsWith(HapagPersonGroupConstants.BaseHapagProfilePrefix);
        }

        public static bool IsALocationGroup(PersonGroup group) {
            if (group.Description != null && group.Description.ToUpper().StartsWith("ZZZ")) {
                //https://controltechnologysolutions.atlassian.net/browse/HAP-868
                return false;
            }
            return group.SuperGroup || group.Name.StartsWith(HapagPersonGroupConstants.BaseHapagLocationPrefix) || IsSuperGroup(group);
        }

        //Region-Area-LocationID-Cost Center
        public static string GetSubCustomerId(PersonGroup group) {
            return HapagPersonGroupConstants.PersonGroupPrefix + DoGet(@group, LocationIdIdx);
        }

        public static string GetCostCenter(PersonGroup group) {
            if (@group == null || group.Description == null) {
                return null;
            }
            var idxToSearch = @group.Description.GetNthIndex('-', CostCenterIdx);
            if (idxToSearch == -1) {
                return null;
            }

            return @group.Description.Substring(idxToSearch + 1);
        }

        private static string DoGet(PersonGroup @group, int idx) {
            if (@group == null) {
                return null;
            }
            var splitString = @group.Description.Split('-');
            if (splitString.Length < 4) {
                return null;
            }
            return splitString[idx];
        }

        public static string GetAllOtherCostCentersAsInQuery(IEnumerable<string> otherCostCenters) {
            var sb = new StringBuilder();
            foreach (var otherCostCenter in otherCostCenters) {
                sb.Append("'").Append(otherCostCenter).Append("'").Append(",");
            }
            return sb.ToString(0, sb.Length - 1);
        }

        //        public static List<HlagAssociationLocation> ConvertToAssociationList(IEnumerable<HlagLocation> hlagLocations) {
        //            var result = hlagLocations.Select(hlagLocation => new HlagAssociationLocation(hlagLocation)).ToList();
        //            result.Sort();
        //            return result;
        //        }



    }
}
