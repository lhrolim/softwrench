(function (angular) {
    "use strict";

angular.module("sw_layout").controller("InvIssueReturnActionController", InvIssueReturnActionController);
function InvIssueReturnActionController($scope) {
    "ngInject";

    $scope.hasBeenReturned = function (matusetransitem) {
        var deltaQty = matusetransitem['quantity'] + matusetransitem['qtyreturned'] >= 0;
        return deltaQty;
    };

    $scope.isReturnHidden = function (matusetransitem) {
        return matusetransitem['issuetype'] != 'RETURN';
    };
}

window.InvIssueReturnActionController = InvIssueReturnActionController;

})(angular);