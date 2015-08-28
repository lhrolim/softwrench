(function (softwrench) {
    "use strict";

    softwrench.controller('FilterOptionController', ["$log", "$scope", "$ionicScrollDelegate", "crudContextService", "crudFilterContextService",
        function ($log, $scope, $ionicScrollDelegate, crudContextService, crudFilterContextService) {

    $scope.showpending = crudFilterContextService.showPending();
    $scope.showdirty = crudFilterContextService.showDirty();
    $scope.showproblems = crudFilterContextService.showProblems();

    $scope.changePending = function () {
        $scope.showpending = !$scope.showpending;
        crudFilterContextService.showPending($scope.showpending);
        crudContextService.refreshGrid();
    }

    $scope.changeDirty = function () {
        $scope.showdirty = !$scope.showdirty;
        crudFilterContextService.showDirty($scope.showdirty);
        crudContextService.refreshGrid();
    }

    $scope.changeProblem = function () {
        $scope.showProblems = !$scope.showProblems;
        crudFilterContextService.showProblems($scope.showProblems);
        crudContextService.refreshGrid();
    }

}]);

})(softwrench);



