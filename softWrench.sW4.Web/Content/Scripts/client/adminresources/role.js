(function (angular) {
    "use strict";

angular.module("sw_layout").controller("RoleController", RoleController);
function RoleController($scope, $http, $templateCache, i18NService) {
    "ngInject";

    function bind(application) {
        $scope.application = application;
        $scope.title = application.title;
        $scope.showList();
    };


    function toDetail() {
        switchMode(true);
    };

    function toList(data) {
        if (data != null) {
            $scope.roles = data;
        }
        switchMode(false);
    };

    function switchMode(mode) {

        $scope.isDetail = mode;
        $scope.isList = !mode;
    }

    $scope.editRole = function (id, name, active) {
        $scope.role = {};
        $scope.role.id = id;
        $scope.role.name = name;
        $scope.role.active = active;
        toDetail();
    };

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

    $scope.delete = function () {
        $('#saveBTN').prop('disabled', 'disabled');
        $http.put(url("api/security/Role"), JSON.stringify($scope.role))
            .success(function (data) {
                $('#saveBTN').removeAttr('disabled');
                toList(data.resultObject);
            })
            .error(function (data) {
                $('#saveBTN').removeAttr('disabled');
                $scope.title = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
            });
    };

    $scope.save = function () {
        $('#saveBTN').prop('disabled', 'disabled');
        $http.post(url("api/security/Role"), JSON.stringify($scope.role))
          .success(function (data) {
              $('#saveBTN').removeAttr('disabled');
              toList(data.resultObject);
          })
          .error(function (data) {
              $('#saveBTN').removeAttr('disabled');
              $scope.title = data || i18NService.get18nValue('general.requestfailed', 'Request failed');
          });
    };

    $scope.new = function () {
        toDetail(true);
    };

    function init() {
        $scope.roles = $scope.resultData;
        toList(null);
    }

    init();

}

window.RoleController = RoleController;

})(angular);