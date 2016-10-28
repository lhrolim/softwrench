(function (angular) {
    "use strict";
    var module = angular.module("sw_layout");
    module.controller("TransactionStatsController", TransactionStatsController);
    module.filter('utcToLocal', Filter);

    function Filter($filter) {
        return function (utcDateString, format) {
            // return if input date is null or undefined
            if (!utcDateString) {
                return;
            }

            // append 'Z' to the date string to indicate UTC time if the timezone isn't already specified
            if (utcDateString.indexOf('Z') === -1 && utcDateString.indexOf('+') === -1) {
                utcDateString += 'Z';
            }

            // convert and format date using the built in angularjs date filter
            return $filter('date')(utcDateString, format);
        };
    }

    TransactionStatsController.$inject = ["$scope", "$http", "restService", "i18NService", "alertService"];

    function TransactionStatsController($scope, $http, restService, i18NService, alertService) {
        "ngInject";
        $scope.fromDateFilter = getFormattedToday();
        $scope.toDateFilter = getFormattedToday();

        $scope.userStatistics = $scope.resultData;
        
        $scope.refresh = function (fromDateFilter, toDateFilter) {
            getStatistics(fromDateFilter, toDateFilter);
        }

        function getFormattedToday() {
            var current = new Date();
            return (current.getMonth() + 1).toString() + '/' + current.getDate().toString() + '/' + current.getFullYear().toString() + ' ' + current.getHours().toString() + ':' + current.getMinutes().toString();
        }


        function getStatistics(fromDateFilter, toDateFilter) {
            const queryParameters = {};

            if (fromDateFilter) {
                queryParameters.fromDateFilter = (new Date(fromDateFilter)).toISOString();
            }

            if (fromDateFilter) {
                queryParameters.toDateFilter = (new Date(toDateFilter)).toISOString();
            }

            restService.getPromise("TransactionStatistics", "GetStatistics", queryParameters).then(response => {
                $scope.userStatistics = response.data.resultObject;
            });
        }
        
        $scope.refresh();
    };
})(angular);