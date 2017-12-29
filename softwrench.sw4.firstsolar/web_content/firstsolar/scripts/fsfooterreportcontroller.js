(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("fsreportfooterController", ["$scope", function ($scope) {

        $scope.url = function (path) {
            return url(path);
        }

    }]);

})(angular);