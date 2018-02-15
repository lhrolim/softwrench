(function (angular) {
    "use strict";

    function ctrl ($scope, crudContextHolderService, searchService) {

        $scope.currentSelectedProfile = crudContextHolderService.getCurrentSelectedProfile($scope.panelid);

        $scope.hasMultiplesProfiles = function () {
            return this.getMultiplesProfiles().length > 1;
        }

        $scope.getMultiplesProfiles = function () {
            const panelId = $scope.panelid || null;
            const isDashboard = $scope.dashboard === "true";
            if (isDashboard) {
                return crudContextHolderService.getAffectedProfilesDashboard($scope.application);
            }
            return crudContextHolderService.getAffectedProfiles(panelId);
        }

        $scope.changeCurrentProfile = function () {
            const panelId = $scope.panelid || null;
            const metadataid = $scope.metadataid || null;
            const isDashboard = $scope.dashboard === "true";

            crudContextHolderService.setCurrentSelectedProfile($scope.currentSelectedProfile, panelId);
            if (!isDashboard) {
                searchService.refreshGrid({}, null, { panelid: panelId, metadataid });
            } else {
                crudContextHolderService.adjustChildSelectedProfiles($scope.currentSelectedProfile);
            }
            
            $scope.$emit(JavascriptEventConstants.ChangeCurrentProfile,$scope.currentSelectedProfile,$scope.dashboard === "true");
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
                    lookup: '=',
                    dashboard: '@',
                    application: '@'
                },

                controller: "GridMultipleProfileController"

            };

            return directive;

        }])
        ;


})(angular);





