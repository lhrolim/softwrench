(function (angular) {
    "use strict";

    function ctrl ($scope, crudContextHolderService, searchService) {

        $scope.currentSelectedProfile = crudContextHolderService.getCurrentSelectedProfile($scope.panelid);

        $scope.hasMultiplesProfiles = function () {
            return this.getMultiplesProfiles().length > 1;
        }

        $scope.getMultiplesProfiles = function () {
            const panelId = $scope.panelid || null;
            return crudContextHolderService.getAffectedProfiles(panelId);
        }

        $scope.changeCurrentProfile = function () {
            const panelId = $scope.panelid || null;
            const metadataid = $scope.metadataid || null;
            crudContextHolderService.setCurrentSelectedProfile($scope.currentSelectedProfile, panelId);
            searchService.refreshGrid({}, null, { panelid: panelId, metadataid });
        }
    }



    angular.module('sw_layout')
        .controller('GridMultipleProfileController', ['$scope', 'crudContextHolderService', 'searchService', ctrl])
        .directive("gridMultipleProfile", ["contextService", function (contextService) {
            "ngInject";
            const directive = {

                restrict: "E",
                templateUrl: contextService.getResourceUrl('/Content/Templates/commands/grid/multipleprofile_selector.html'),

                scope: {
                    panelid: '=',
                    metadataid: '=',
                    lookup: '='
                },

                controller: "GridMultipleProfileController"

            };

            return directive;

        }])
        ;


})(angular);





