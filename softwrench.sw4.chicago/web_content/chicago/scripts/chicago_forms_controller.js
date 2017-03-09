(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("chicagoForms", chicagoForms);
    chicagoForms.$inject = ["$scope", "chicagoformsService"];
    function chicagoForms($scope, chicagoformsService) {
        console.log(chicagoformsService);

        $scope.download = function() {
        }
    }
})(angular);
