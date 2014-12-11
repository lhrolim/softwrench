function InvIssueReturnActionController($scope, contextService, alertService, modalService, restService, redirectService) {

    $scope.hasBeenReturned = function (matusetransitem) {
        var data = matusetransitem['fields'];
        var deltaQty = data['quantity'] + data['qtyreturned'] >= 0;
        return deltaQty;
    };

    $scope.isReturnHidden = function (matusetransitem) {
        var data = matusetransitem['fields'];
        return data['issuetype'] != 'RETURN';
    };

}

