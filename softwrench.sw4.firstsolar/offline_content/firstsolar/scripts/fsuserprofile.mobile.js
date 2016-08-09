(function (angular) {
    "use strict";

    angular.module("softwrench").controller("fsUserProfile", ["$scope", "fsUserProfileService", "routeService", "$ionicPopup", function ($scope, fsUserProfileService, routeService, $ionicPopup) {

        function init() {
            const current = fsUserProfileService.getUserProfileViewModel();

            $scope.profile = {
                original: angular.copy(current),
                current: current
            };    
        }
        

        $scope.hasChanges = function() {
            return !angular.equals($scope.profile.original, $scope.profile.current);
        };

        $scope.navigateBack = function () {
            routeService.go("main.home");
        };

        $scope.cancel = function() {
            $ionicPopup.confirm({ title: "Cancel", template: "Are you sure you want to cancel unsaved changes?" })
                .then(res => res ? init() : null);
        };

        $scope.saveProfile = function () {
            fsUserProfileService.saveUserProfile($scope.profile.current)
                .then(() => $ionicPopup.alert({ title: "Success", template: "Profile saved successfully" }))
                .then(init);
        };

        init();

    }]);

})(angular);