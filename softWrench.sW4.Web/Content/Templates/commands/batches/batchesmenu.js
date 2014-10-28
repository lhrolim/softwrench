function BatchesController($scope, $http, redirectService) {

    $scope.manageBatches= function (event) {
        redirectService.goToApplication("_wobatch", "list", {});
    }

}