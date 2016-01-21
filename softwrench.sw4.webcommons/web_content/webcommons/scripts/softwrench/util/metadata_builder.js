(function (angular) {
    "use strict";

    function entityBuilderController($scope, $http, i18NService) {

        $scope.generateData = function () {

            $scope.hasdata = false;
            $scope.HasError = false;

            $http.get(url("/api/generic/EntityMetadata/Build?tablename=" + $scope.table))
                .success(function (data) {
                    $scope.hasdata = true;
                    $scope.metadata = data.metadata;
                })
                .error(function (data) {
                    $scope.error = data.error;
                    $scope.hasdata = true;
                });
        };

        $scope.i18N = function (key, defaultValue, paramArray) {
            return i18NService.get18nValue(key, defaultValue, paramArray);
        };
    }

    angular.module('sw_layout').controller("EntityBuilderController", ["$scope", "$http", "i18NService", entityBuilderController]);

})(angular);