(function (angular) {
    "use strict";

    angular.module("softwrench")
        .controller("SupportController", ["$scope", "supportService", function ($scope, supportService) {

            $scope.sendLogFiles = function () {
                supportService.requestLogReporting();
            };

        }]);

})(angular);