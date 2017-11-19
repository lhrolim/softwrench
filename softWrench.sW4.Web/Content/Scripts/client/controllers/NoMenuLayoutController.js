(function (angular) {
    'use strict';

    function NoMenuLayoutController($scope, contextService) {
        $scope.resourceUrl = function (path) {
            return contextService.getResourceUrl(path);
        }

        $scope.$on("sw_loadmenu", function (event, menuModel) {
            contextService.insertIntoContext("commandbars", menuModel.commandBars);
        });
    }

    NoMenuLayoutController.$inject = ['$scope', 'contextService'];
    angular.module('sw_layout').controller('NoMenuLayoutController', NoMenuLayoutController);

})(angular);
