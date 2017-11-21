
(function (angular) {
    'use strict';

    angular.module('sw_layout').controller('dispatchbuttonctrl', ["$scope",'applicationService', 'crudContextHolderService', dispatchbuttonctrl]);

    function dispatchbuttonctrl($scope,applicationService, crudContextHolderService) {

        $scope.dispatch =() => {
            const datamap = crudContextHolderService.rootDataMap();
            datamap["#dispatching"] = true;
            return applicationService.save({ datamap});
        }



    }
})(angular);
