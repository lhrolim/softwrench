(function (angular) {
    "use strict";

    function sqlClientController($scope, $http, localStorageService) {
        //Constants and local variables
        const HistoryCurrentKey = "sqlclient_current";
        const HistoryDataKey = "sqlclient_history";
        const HistoryDefaultValue = 1;

        //Scope properties
        $scope.queryString = '';
        $scope.executionMessage = '';

        $scope.hasData = false;
        $scope.hasError = false;
        $scope.limitRows = true;
        $scope.limitFetchRowsCount = 10;

        $scope.dataSource = 'swdb';
        $scope.dataSources = [           
            { name: "SWDB", value: "swdb" },
            { name: "Maximo", value: "maximo" }
        ];

        $scope.headers = [];
        $scope.resultSet = null;

        $scope.historyCurrent = HistoryDefaultValue;
        $scope.queryHistory = [];
        $scope.queryHistorySelection = null;

        // scope methods       

        $scope.runSqlQuery = function () {
            var status = validateSqlQuery();
            if (status) {
                var rowLimit = $scope.limitRows ? $scope.limitFetchRowsCount : 0;
                var selectionText = getQuerySelection();

                var query = selectionText ? selectionText : $scope.queryString;

                $http.get(url("/api/generic/sqlclient/executequery?query=" + query + "&datasource=" + $scope.dataSource + "&limit=" + rowLimit))
                    .then(function (response) {
                        const data = response.data;
                        if (data.hasErrors) {
                            $scope.executionMessage = data.executionMessage;
                            $scope.hasData = false;
                            $scope.hasError = true;
                        } else {
                            processResultSet(data);
                            addQueryToHistory(query);
                        }
                    })
                    .catch(function (data) {
                        $scope.executionMessage = "There was an error whilst processing your request. Please try again.";
                        $scope.hasData = false;
                        $scope.hasError = true;
                    });
            } else {

            }
        };

        $scope.selectHistoryQuery = function (selectedQuery){
            if (selectedQuery) {
                for (let i = 0; i < $scope.queryHistory.length; i++) {
                    if ($scope.queryHistory[i].key === selectedQuery[0]) {
                        $scope.queryString = $scope.queryHistory[i].data.query;
                    }
                }
            }
        };

        $scope.clearHistory = function () {
            clearClientHostory();
        };
                
        function processResultSet (data) {
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

        function readClientHistory() {
            $scope.historyCurrent = localStorageService.get(HistoryCurrentKey);

            if (!$scope.historyCurrent) {
                $scope.historyCurrent = HistoryDefaultValue;
                localStorageService.put(HistoryCurrentKey, $scope.historyCurrent);
            }

            for (var i = HistoryDefaultValue; i <= $scope.historyCurrent; i++) {
                let key = HistoryDataKey + i.toString();
                let data = localStorageService.get(key);

                if (data) {
                    $scope.queryHistory.push({
                        "key": key,
                        "data": data
                    });

                } else {
                    $scope.historyCurrent = i - 1;
                    break;
                }
            }
        };

        function addQueryToHistory(query) {    
            let randomKey = HistoryDataKey + $scope.historyCurrent.toString();
            let data = {
                "query": query,
                "executionTime": new Date().toDateString()
            }
         
            $scope.queryHistory.push({
                "key": randomKey,
                "data": data
            });

            localStorageService.put(randomKey, data);
            localStorageService.put(HistoryCurrentKey, $scope.historyCurrent);

            $scope.historyCurrent += 1;
        };

        function clearClientHostory() {
            for (var i = HistoryDefaultValue; i <= $scope.historyCurrent; i++) {
                let key = HistoryDataKey + i.toString();
                localStorageService.remove(key);

                $scope.queryHistory.pop();
            }

            $scope.historyCurrent = HistoryDefaultValue;
            localStorageService.put(HistoryCurrentKey, $scope.historyCurrent);
        };

        function validateSqlQuery() {
            if ($scope.queryString === undefined || $scope.queryString.nullOrEmpty()) {
                $scope.hasData = false;
                $scope.hasError = true;
                $scope.executionMessage = "SQL Query cannot be blank.";
                return false;
            }

            return true;
        };

        function getQuerySelection() {
            let elem = $("#queryTextArea");
            if (typeof elem != "undefined") {
                let start = elem[0].selectionStart;
                let end = elem[0].selectionEnd;
                return elem.val().substring(start, end);
            } else {
                return '';
            }
        }

        //Init the client
        readClientHistory();
    };

    var mod = angular.module('sw_layout');

    mod.controller("SQLClientController", ["$scope", "$http", "localStorageService", sqlClientController]);

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
                if(event.which === 17) {
                    ctrlDown = false;
                }
            });
        };
    });

})(angular);