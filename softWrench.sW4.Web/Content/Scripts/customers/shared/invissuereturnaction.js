(function (angular) {
    "use strict";

angular.module("sw_layout").controller("InvIssueReturnActionController", InvIssueReturnActionController);
function InvIssueReturnActionController($scope) {
    "ngInject";

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

window.InvIssueReturnActionController = InvIssueReturnActionController;

})(angular);