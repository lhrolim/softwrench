(function (angular) {
    "use strict";

    function swgasRequestController($scope) {
        $scope.newRequest = () => window.location = url("/swgasrequest");

        $scope.userid = () => JSON.parse(homeModel.ResultDataJSON).resultObject;
    }

    swgasRequestController.$inject = ["$scope"];

    angular.module("sw_layout").controller("SwgasRequestController", swgasRequestController);

})(angular);
