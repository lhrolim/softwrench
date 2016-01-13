(function (angular) {
    "use strict";

angular.module("sw_layout").controller("ReceivingController", ReceivingController);
function ReceivingController($scope, contextService, alertService, searchService) {
    "ngInject";

    $scope.receive = function (compositionitem) {
        if (compositionitem['orderqty'] <= compositionitem['receivedqty']) {
            alertService.alert("Cannot receive more than you ordered !! Receipts Completed");
            return;
        }
        if (compositionitem['receivedqty'] == null) {
            //need to perform the query to get the total quantity due -- Extract everything into a function of poreceiving service later
            var qtydue = 0;
            var searchData = {
                ponum: compositionitem['ponum'],
                polinenum: String(compositionitem['polinenum'])
            };
            searchService.searchWithData("materialrecords", searchData).success(function(data) {
                var resultObject = data.resultObject;
                var totalquantityreceived = 0;
                var i = 0;
                for (i = 0;i< resultObject.length;i++) {
                    totalquantityreceived = totalquantityreceived + resultObject[i]['fields']['quantity'];
                }
                qtydue = compositionitem['orderqty'] - totalquantityreceived;
                // prepopulate the values for the matrectrans record
                var clonedItem = {};
                angular.copy(compositionitem, clonedItem);
                var originalPoNum = clonedItem['ponum'];
                var originalPoLineNum = String(clonedItem['polinenum']);
                clonedItem['polinenum'] = originalPoLineNum;
                clonedItem['#qtydue'] = qtydue;
                $scope.$emit("sw.composition.edit", clonedItem);
            });
            //open a schema to submit a matrectrans record
        }
        return;
    };
}

window.ReceivingController = ReceivingController;

})(angular);