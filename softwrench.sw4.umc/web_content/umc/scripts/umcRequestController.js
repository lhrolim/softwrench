(function (angular) {
    "use strict";

    function UmcRequestController($scope) {
        $scope.newRequest = () => window.location = url("/umcrequest");

        $scope.userid = () => JSON.parse(homeModel.ResultDataJSON).resultObject;
    }

    UmcRequestController.$inject = ["$scope"];

    angular.module("sw_layout").controller("UmcRequestController", UmcRequestController);

})(angular);
