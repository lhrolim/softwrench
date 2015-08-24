(function (angular) {
    'use strict';

    function NoMenuLayoutController($scope, contextService) {
        $scope.resourceUrl = function (path) {
            return contextService.getResourceUrl(path);
        }
    }

    NoMenuLayoutController.$inject = ['$scope', 'contextService'];
    angular.module('sw_layout').controller('NoMenuLayoutController', NoMenuLayoutController);

})(angular);
