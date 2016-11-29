(function (angular) {
    "use strict";

    angular.module("sw_layout").controller("ProfileController", ProfileController);


    function ProfileController($scope, crudContextHolderService, userService, alertService, redirectService) {
        "ngInject";

        var app = angular.module('plunker', ['ui.multiselect']);

        $scope.doubleClickedProfile = function () {
            var profile = $scope.selectedprofiles[0] || $scope.selectedavailableprofiles[0];
            goToProfileDetails(profile);
        };

        $scope.canAssignSecurity = function () {
            var schema = crudContextHolderService.currentSchema();
            return schema.schemaId !== 'myprofiledetail' || userService.hasRole(["sysadmin"]);
        };

        $scope.addSelectedProfiles = function (selectedavailableprofiles) {
            if (selectedavailableprofiles === undefined || selectedavailableprofiles.length === undefined || selectedavailableprofiles.length === 0) {
                alertService.alert("Select a security group from \"Available Security Groups\" list before adding");
            } else {
                $scope.parentDatamap['#profiles'] = $scope.parentDatamap['#profiles'].concat(selectedavailableprofiles);
                $scope.profiles = $scope.parentDatamap['#profiles'];
                var availableProfilesArr = $scope.availableprofiles;
                $scope.availableprofiles = availableProfilesArr.filter(function (item) {
                    return selectedavailableprofiles.indexOf(item) === -1;
                });
            }
        };

        $scope.removeSelectedProfiles = function (selectedprofiles) {
            if (selectedprofiles === undefined || selectedprofiles.length === undefined || selectedprofiles.length === 0) {
                alertService.alert("Select a security group from \"Active Security Groups\" list before deleting");
            } else {
                $scope.availableprofiles = $scope.availableprofiles.concat(selectedprofiles);
                var userProfiles = $scope.parentDatamap['#profiles'];
                $scope.parentDatamap['#profiles'] = userProfiles.filter(function (item) {
                    return selectedprofiles.indexOf(item) === -1;
                });
                $scope.profiles = $scope.parentDatamap['#profiles'];
            }
        };

        function goToProfileDetails(profile) {
            var schema = crudContextHolderService.currentSchema();
            if (profile && schema.schemaId !== 'myprofiledetail' && userService.hasRole(["sysadmin"])) {
                redirectService.goToApplicationView("_UserProfile", "detail", "edit", "Details", { id: profile.id, popupmode: 'browser' });
            }
        };

        function initUser() {
            $scope.parentDatamap = $scope.datamap;
            $scope.availableprofiles = $scope.parentDatamap['#availableprofiles'];
            $scope.selectedavailableprofiles = [];
            $scope.profiles = $scope.parentDatamap['#profiles'];
            $scope.selectedprofiles = [];
            $scope.availableprofilesOriginal = $scope.parentDatamap['#availableprofiles'];
        };

        initUser();
    }

    window.ProfileController = ProfileController;

})(angular);
