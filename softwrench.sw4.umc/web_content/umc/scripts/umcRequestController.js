(function (angular) {
    "use strict";

    function UmcRequestController($scope) {
        $scope.newRequest = () => window.location.pathname = url("/umcrequest");
    }

    UmcRequestController.$inject = ["$scope"];

    angular.module("sw_layout").controller("UmcRequestController", UmcRequestController);

})(angular);
