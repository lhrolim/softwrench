﻿

function AuditController($scope, $http, $templateCache, pwdenforceService, i18NService) {

    $scope.addSelectedProfiles = function (availableprofilesselected) {
        $scope.user.profiles = $scope.user.profiles.concat(availableprofilesselected);
        var availableProfilesArr = $scope.availableprofiles;
        $scope.availableprofiles = availableProfilesArr.filter(function (item) {
            return availableprofilesselected.indexOf(item) === -1;
        });
    };

    $scope.removeSelectedProfiles = function (selectedProfiles) {
        $scope.availableprofiles = $scope.availableprofiles.concat(selectedProfiles);
        var userProfiles = $scope.user.profiles;
        $scope.user.profiles = userProfiles.filter(function (item) {
            return selectedProfiles.indexOf(item) === -1;
        });
    };

    $scope.addConstraint = function () {
        $scope.profile.dataConstraints.push({ id: null, Entity: "", whereClause: "", isactive: true });
    };

    $scope.removeConstraint = function (constraint) {
        var dataConstraints = $scope.profile.dataConstraints;
        var idx = dataConstraints.indexOf(constraint);
        if (idx > -1) {
            dataConstraints.splice(idx, 1);
        }
    };

    function toDetail() {
        switchMode(true);
    };

    function toList(data) {
        $scope.availableroles = $scope.availablerolesOriginal;
        $scope.availableprofiles = $scope.availableprofilesOriginal;

        if (data != null) {
            $scope.listObjects = data;
        }
        switchMode(false);
    };

    function switchMode(mode) {

        $scope.isDetail = mode;
        $scope.isList = !mode;

    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.showList = function () {
        $scope.searchData = {};
        $scope.selectPage(1);
    };

    $scope.cancel = function () {
        toList(null);
    };

    $scope.new = function () {
        $scope.user = {};
        $scope.user.profiles = [];
        $scope.user.customRoles = [];
        $scope.user.customConstraints = [];
        toDetail(true);
    };

    function initUser() {
        var data = $scope.resultData;
        $scope.listObjects = data.users;
        $scope.$emit('sw_titlechanged', i18NService.get18nValue('general.usersetup','User Setup'));
        $scope.availableprofiles = data.profiles;
        $scope.availableprofilesselected = {};
        $scope.selectedProfiles = {};
        $scope.availableprofilesOriginal = data.profiles;

        $scope.availableroles = data.roles;
        $scope.availablerolesOriginal = data.roles;
        $scope.selectedroles = {};
        $scope.availablerolesselected = {};

        toList(null);
    };

    initUser();
}


