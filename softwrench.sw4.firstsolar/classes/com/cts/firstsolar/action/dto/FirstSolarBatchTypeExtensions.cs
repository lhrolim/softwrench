using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.dto {
    public static class FirstSolarBatchTypeExtensions {

        public static string GetSpreadhSheetSchema (this FirstSolarBatchType batchType) {
            return batchType.Equals(FirstSolarBatchType.Asset) ? "batchAssetSpreadSheet" : "batchLocationSpreadSheet";
        }

        public static string GetUserIdName(this FirstSolarBatchType batchType) {
            return batchType.Equals(FirstSolarBatchType.Asset) ? "assetnum" : "location";
        }

        public static string GetSuccessMessage(this FirstSolarBatchType batchType, int count) {

            var baseName = batchType.Equals(FirstSolarBatchType.Asset) ? "asset" : "workorder";
            if (count > 1) {
                return string.Format(count + " {0}s created successfully", baseName);
            }
            return string.Format(" {0} created successfully", baseName);
        }



    }
}
