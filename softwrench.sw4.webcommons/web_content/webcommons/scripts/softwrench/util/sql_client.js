(function (angular) {
    "use strict";

    function sqlClientController($scope, $http, i18NService) {
        //Scope properties
        $scope.queryString = '';
        $scope.executionMessage = '';

        $scope.hasData = false;
        $scope.hasError = false;
        $scope.limitRows = true;

        $scope.dataSource = 'swdb';
        $scope.dataSources = [           
            { name: "SWDB", value: "swdb" },
            { name: "Maximo", value: "maximo" }
        ];

        $scope.headers = [];
        $scope.resultSet = null;
       

        // scope methods
        $scope.validateSqlQuery = function () {
            if ($scope.queryString === undefined || $scope.queryString.nullOrEmpty()) {
                $scope.hasData = false;
                $scope.hasError = true;
                $scope.executionMessage = "SQL Query cannot be blank.";
                return false;
            }

            return true;
        };

        $scope.runSqlQuery = function () {
            var status = $scope.validateSqlQuery();
            if (status) {
                var rowLimit = $scope.limitRows ? 10 : 0;
                $http.get(url("/api/generic/sqlclient/executequery?query=" + $scope.queryString + "&datasource=" + $scope.dataSource + "&limit=" + rowLimit))
                    .success(function (data) {
                        if (data.hasErrors) {
                            $scope.executionMessage = data.executionMessage;
                            $scope.hasData = false;
                            $scope.hasError = true;
                        } else {
                            $scope.processResultSet(data);
                        }

                    })
                    .error(function (data) {
                        $scope.executionMessage = "There was an error whilst processing your request. Please try again.";
                        $scope.hasData = false;
                        $scope.hasError = true;
                    });
            } else {

            }
        };

        $scope.processResultSet = function (data) {
            if (data !== null && data.resultSet !== null && data.resultSet.length > 0) {
                $scope.resultSet = data.resultSet;
                $scope.hasData = true;
                $scope.hasError = false;
                $scope.executionMessage = data.executionMessage;

                //clear the headers
                $scope.headers.splice(0, $scope.headers.length);
                for (var column in data.resultSet[0]) {
                    $scope.headers.push(column);
                }
            } else {
                $scope.executionMessage = data.executionMessage;
                $scope.hasData = false;
                $scope.hasError = false;
            }
        };

        $scope.i18N = function (key, defaultValue, paramArray) {
            return i18NService.get18nValue(key, defaultValue, paramArray);
        };
    }

    var mod = angular.module('sw_layout');

    mod.controller("SQLClientController", ["$scope", "$http", "i18NService", sqlClientController]);

    mod.directive('shortenter', function(){
        return function (scope, element) {
            var ctrlDown = false;
            element.bind("keydown", function (event) {

                // sets ctrlDown to true if the ctrl key is pressed
                if(event.which === 17) {
                    ctrlDown = true;
                }

                // run the query if "enter" is pressed and "ctrl" is being held down
                if (event.which === 13 && ctrlDown) {
                    event.preventDefault();
                    scope.runSqlQuery();
                    scope.$apply();
                }
            });

            // sets ctrlDown to false if the shift key has been released
            element.bind("keyup", function (event) {
                if(event.which === 16) {
                    ctrlDown = false;
                }
            });
        };
    });

})(angular);