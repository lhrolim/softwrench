var app = angular.module('sw_layout');

app.controller('GridMultipleProfileController', ['$scope', 'crudContextHolderService','searchService',
        function ($scope, crudContextHolderService, searchService) {

            $scope.currentSelectedProfile = crudContextHolderService.getCurrentSelectedProfile();

            $scope.hasMultiplesProfiles = function () {
                return crudContextHolderService.getAffectedProfiles().length > 1;
            }

            $scope.getMultiplesProfiles = function () {
                return crudContextHolderService.getAffectedProfiles();
            }

            $scope.changeCurrentProfile = function() {
                crudContextHolderService.setCurrentSelectedProfile($scope.currentSelectedProfile);
                searchService.refreshGrid({});

            }


        }
]);