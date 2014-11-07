function InvIssueReturnActionController($scope, contextService, alertService, modalService, restService, redirectService) {

    $scope.hasBeenReturned = function (matusetransitem) {
        var data = matusetransitem['fields'];
        if ((data['quantity'] - data['qtyreturned']) <= 0) {
            return true;
        }

        return false;
    };

    $scope.updateOpacity = function (matusetransitem) {
        if ($scope.hasBeenReturned(matusetransitem)) {
            return "low-opacity gray";
        }

        return "blue";
    };

    $scope.isReturnHidden = function (matusetransitem) {
        var data = matusetransitem['fields'];
        if (data['issuetype'] == 'RETURN') {
            return false;
        }
        return true;
    };

}

