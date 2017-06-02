namespace softwrench.sw4.firstsolar.classes.com.cts.firstsolar.action.dto {
    public static class FirstSolarBatchTypeExtensions {

        public static string GetSpreadhSheetSchema(this FirstSolarBatchType batchType) {
            return batchType.Equals(FirstSolarBatchType.Asset) ? "batchAssetSpreadSheet" : "batchLocationSpreadSheet";
        }

        public static string GetUserIdName(this FirstSolarBatchType batchType) {
            return batchType.Equals(FirstSolarBatchType.Asset) ? "assetnum" : "location";
        }

        public static string GetSuccessMessage(this FirstSolarBatchType batchType, int count) {


            if (count > 1) {
                return count + " Workorders created successfully";
            }
            return "Workorder created successfully";
        }



    }
}
