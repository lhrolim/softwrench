softwrench.controller('FilterOptionController', function ($log, $scope, $ionicScrollDelegate, crudContextService) {

    'use strict';

    $scope.showpending = crudContextService.showPending();

    $scope.changePending = function () {
        $scope.showpending = !$scope.showpending;
        crudContextService.showPending($scope.showpending);
        crudContextService.refreshGrid();
    }


}
);