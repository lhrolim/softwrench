(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("RoutePageController", RoutePageController);
    RoutePageController.$inject = ["$scope", "$location"];
    function RoutePageController($scope, $location) {
        "ngInject";

        const data = $scope.resultData;
        $scope.pageNotFound = data.pageNotFound;
        $scope.datamaps = data.datamaps;

        $scope.info = `More than one ${data.applicationTitle} was found. Please choose the desired site:`;

        $scope.title = function (datamap) {
            return datamap[data.siteIdFieldName || data.idFieldName];
        }

        $scope.route = function (datamap) {
            const id = datamap[data.idFieldName];
            const bar = data.contextPath.endsWith("/") ? "" : "/";
            const path = `${data.contextPath}${bar}web/${data.applicationName}/uid/${id}`;
            window.location.hash = "";
            window.location.pathname = path;
        }
    };
})(angular);