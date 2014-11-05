function InvIssueReturnActionController($scope, contextService) {

    $scope.hasBeenReturned = function (matusetransitem) {
        var data = matusetransitem['fields'];
        if ((data['quantity'] + data['qtyrequested']) == 0) {
            return true;
        }

        return false;
    };

    $scope.return = function (matusetransitem) {
        var test = matusetransitem;
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

