(function (window) {
    "use strict";

angular.module("sw_layout").controller("UserProfileController", UserProfileController);
function UserProfileController($scope, $http, $templateCache, i18NService) {

    var app = angular.module('plunker', ['ui.multiselect']);

    $scope.addSelectedRoles = function (availablerolesselected) {
        $scope.profile.roles = $scope.profile.roles.concat(availablerolesselected);
        var availableRolesArr = $scope.availableroles;
        $scope.availableroles = availableRolesArr.filter(function (item) {
            return availablerolesselected.indexOf(item) === -1;
        });
    };

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.removeSelectedRoles = function (selectedRoles) {
        $scope.availableroles = $scope.availableroles.concat(selectedRoles);
        var profileRoles = $scope.profile.roles;
        $scope.profile.roles = profileRoles.filter(function (item) {
            return selectedRoles.indexOf(item) === -1;
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
        if (data != null) {
            $scope.listObjects = data;
        }
        switchMode(false);
    };

    function switchMode(mode) {
        $scope.errorMessage = null;
        $scope.isDetail = mode;
        $scope.isList = !mode;
    }

    $scope.editProfile = function (profile) {
        $scope.profile = profile;
        var availableRolesArr = $scope.availableroles;
        $scope.availableroles = availableRolesArr.filter(function (item) {
            var roles = profile.roles;
            for (var i = 0; i < roles.length; i++) {
                if (item["id"] == roles[i]["id"]) {
                    return false;
                }
            }
            return true;
        });
        toDetail();
    };



    $scope.showList = function () {
        $scope.searchData = {};
        $scope.selectPage(1);
    };

    $scope.cancel = function () {
        toList(null);
    };

    $scope.delete = function () {
        $('#saveBTN').prop('disabled', 'disabled');
        $http.put(url("api/security/UserProfile"), JSON.stringify($scope.profile))
            .success(function (data) {
                $('#saveBTN').removeAttr('disabled');
                toList(data);
            })
            .error(function (data) {
                $('#saveBTN').removeAttr('disabled');
                $scope.errorMessage = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
            });
    };

    $scope.save = function () {
        $('#saveBTN').prop('disabled', 'disabled');
        $http.post(url("api/security/UserProfile"), JSON.stringify($scope.profile))
          .success(function (data) {
              $('#saveBTN').removeAttr('disabled');
              toList(data);
          })
          .error(function (data) {
              $('#saveBTN').removeAttr('disabled');
              $scope.errorMessage = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
          });
    };

    $scope.new = function () {
        $scope.profile = {};
        $scope.profile.roles = [];
        $scope.profile.dataConstraints = [];
        toDetail(true);
    };

    function init() {
        var data = $scope.resultData;
        $scope.listObjects = data.profiles;
        $scope.availableroles = data.roles;
        $scope.availablerolesOriginal = data.roles;
        $scope.selectedroles = {};
        $scope.availablerolesselected = {};
        toList(null);
    }

    init();

}

window.UserProfileController = UserProfileController;

})(window);