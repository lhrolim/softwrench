(function() {
    "use strict";

    softwrench.controller("SyncDetailController",
        ["$scope", "$stateParams", "synchronizationOperationService", "routeService",
        function ($scope, $stateParams, syncopService, routes) {

            $scope.operation = {}

            $scope.batchItems = [];

            var loadSyncOperation = function() {
                var opId = $stateParams.id;
                syncopService.getOperation(opId)
                    .then(function (operation) {
                        $scope.operation = operation;
                        return syncopService.getBatchItems(operation);
                    })
                    .then(function (items) {
                        if (!items) return items;
                        return items.map(function (item) {
                            if (item.problem) {
                                item.simpleproblem = { message: item.problem.message };
                            }
                            return item;
                        });
                    })
                    .then(function(items) {
                        $scope.batchItems = items;
                    });
            };

            $scope.goBack = function() {
                routes.go("main.home");
            };

            $scope.solveProblems = function() {
                // TODO: resubmit $scope.operation's Batches
            };

            loadSyncOperation();


    }]);

})();