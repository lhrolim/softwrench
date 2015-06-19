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
                        $scope.batchItems = items;
                    });
            };

            $scope.goBack = function() {
                routes.go("main.home");
            };

            loadSyncOperation();


    }]);

})();