function ReceivingController($scope,contextService,alertService,searchService) {
    $scope.receive = function(compositionitem){
        if (compositionitem['orderqty'] <= compositionitem['receivedqty']) {
            alertService.alert("Cannot receive more than you ordered !! Receipts Completed");
            return;
        }
        if (compositionitem['receivedqty'] == null) {
            //need to perform the query to get the total quantity due

            // prepopulate the values for the matrectrans record

            //open a schema to submit a matrectrans record
        }
        return;
    };
}