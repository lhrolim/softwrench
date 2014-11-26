function InvIssueReturnActionController($scope, contextService, alertService, modalService, restService, redirectService) {

    $scope.hasBeenReturned = function (matusetransitem) {
        var data = matusetransitem['fields'];
        var deltaQty = data['quantity'] + data['qtyreturned'] >= 0;
        return deltaQty;
    };

    $scope.updateOpacity = function (matusetransitem) {
        if ($scope.hasBeenReturned(matusetransitem)) {
            return "low-opacity gray";
        }

        return "black";
    };

    $scope.isReturnHidden = function (matusetransitem) {
        var data = matusetransitem['fields'];
        return data['issuetype'] != 'RETURN';
    };

}

