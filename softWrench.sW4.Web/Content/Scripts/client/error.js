(function (angular) {
    "use strict";

angular.module("sw_layout").controller("ErrorController", ErrorController);
function ErrorController($scope, i18NService) {
    "ngInject";

    $scope.i18N = function (key, defaultValue, paramArray) {
        return i18NService.get18nValue(key, defaultValue, paramArray);
    };
};

window.ErrorController = ErrorController;

})(angular);