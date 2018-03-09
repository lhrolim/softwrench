(function (angular) {
    "use strict";

    function swgasRequestController($scope, swgasNoLoginSubmitService) {
        $scope.newRequest = () => window.location = url("/swgasrequest");

        $scope.userid = () => JSON.parse(homeModel.ResultDataJSON).resultObject;

        $scope.submit= () =>  {
            swgasNoLoginSubmitService.submit();
        }
    }




    swgasRequestController.$inject = ["$scope", "swgasNoLoginSubmitService"];

    angular.module("sw_layout").controller("swgasSubmitCtrl", swgasRequestController);

})(angular);
