// /// <reference path="maincontroller.js" />
(function (softwrench) {
    "use strict";

    softwrench.controller("SyncOperationHistoryController",
        ["$scope", "synchronizationOperationService", "routeService",
        function($scope, synchronizationOperationService, routeService) {

            $scope.operationList = [];

            $scope.paginationData = {
                currentPage: 1,
                pageSize: 10,
                hasMoreAvailable: true
            };

            $scope.goBack = function () {
                routeService.go("main.home");
            };

            $scope.openDetail = function(operation) {
                routeService.go("main.syncoperationdetail", { id: operation.id });
            };

            // infinite scroll
            $scope.loadPagedList = function () {
                synchronizationOperationService.getSyncList($scope.paginationData.currentPage)
                    .then(function (operations) {
                        // update pagination data
                        $scope.paginationData.hasMoreAvailable = (operations && operations.length >= $scope.paginationData.hasMoreAvailable);
                        $scope.paginationData.currentPage += 1;
                        // update list
                        $scope.operationList = $scope.operationList.concat(operations);
                    })
                    .finally(function () {
                        $scope.$broadcast('scroll.infiniteScrollComplete');
                    });
            };
        }
    ]);

})(softwrench);