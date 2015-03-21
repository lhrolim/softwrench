﻿function MyProfileController($scope, $http, $templateCache, i18NService, $rootScope, redirectService, pwdenforceService) {

    init($scope.resultData);

    function init(data) {
        if (data != null) {
            $scope.currentUser = data.user;
            i18NService.changeCurrentLanguage($scope.currentUser.language);
            $scope.$emit('sw_titlechanged', i18NService.get18nValue('general.profiledetails', 'Profile Details'));
            $scope.isMyProfile = true;
            $scope.isHapag = $rootScope.clientName == 'hapag' ? true : false;
            $scope.canChangeLanguage = $scope.isHapag && data.canChangeLanguage;
            fillRestrictions(data);
            fillLanguage();
        }
    }

    function fillRestrictions(data) {
        $scope.restrictions = {};
        $scope.restrictions = data.restrictions;
        $scope.canViewRestrictions = $rootScope.clientName == 'hapag' && data.canViewRestrictions;
    }

    function fillLanguage() {
        $scope.languages = [
               { value: 'EN', text: $scope.i18N('language.english', 'English') },
               { value: 'DE', text: $scope.i18N('language.german', 'German') },
               { value: 'ES', text: $scope.i18N('language.spanish', 'Spanish') }
        ];
        $scope.language = { selected: null };
        for (var i = 0; i < $scope.languages.length; i++) {
            if ($scope.languages[i].value == $scope.currentUser.language) {
                $scope.language = { selected: $scope.languages[i] };
                break;
            }
        }
    }

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };

    $scope.saveMyProfile = function () {
        if (!pwdenforceService.checker($scope.currentUser.password, $scope.currentUser.password2))
            return;

        $http({
            method: "GET",
            url: url("api/security/User/" + $scope.currentUser.dbId)
        })
        .success(function (user) {
            fillUserToSave(user);
            $('#saveMyProfileBTN').prop('disabled', 'disabled');

            $http.post(url("api/security/User"), user)
                .success(function () {
                    $('#saveMyProfileBTN').removeAttr('disabled');
                    resetuserinfo();
                    $('html, body').animate({ scrollTop: 0 }, 'fast');
                })
                .error(function (data) {
                    $('#saveMyProfileBTN').removeAttr('disabled');
                    $scope.title = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
                });
        });
    };

    function resetuserinfo() {
        var urlToInvoke = redirectService.getActionUrl('User', 'MyProfile', null);
        $http.get(urlToInvoke).
        success(function (data, status, headers, config) {
            init(data.resultObject);
        }).
        error(function (data, status, headers, config) {
            var error = "Error " + status;
        });
    }

    function fillUserToSave(user) {
        var password = $scope.currentUser.password;
        if (!nullOrUndef(password)) {
            user.password = password;
        } else {
            user.password = null;
        }
        user.firstName = $scope.currentUser.firstName;
        user.lastName = $scope.currentUser.lastName;
        user.siteId = $scope.currentUser.siteId;
        user.email = $scope.currentUser.email;
        user.department = $scope.currentUser.department;
        user.phone = $scope.currentUser.phone;
        if (!nullOrUndef($scope.language.selected)) {
            $scope.currentUser.language = $scope.language.selected.value;
        }
        user.language = $scope.currentUser.language;
    }
}