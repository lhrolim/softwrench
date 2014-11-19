function ReceivingController($scope,contextService,alertService,searchService) {
    $scope.receive = function(compositionitem){
        if (compositionitem['orderqty'] <= compositionitem['receivedqty']) {
            alertService.alert("Cannot receive more than you ordered !! Receipts Completed");
            return;
        }
        if (compositionitem['receivedqty'] == null) {
            //need to perform the query to get the total quantity due -- Extract everything into a function of poreceiving service later
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
                var qtydue = compositionitem['orderqty'] - totalquantityreceived;
            });
            // prepopulate the values for the matrectrans record

            //open a schema to submit a matrectrans record
        }
        return;
    };
}