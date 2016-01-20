(function (angular) {
    "use strict";

    angular.module("sw_layout")
    .directive("stringToNumber", function () {
        return {
            restrict: "A",
            require: "ngModel",
            link: function (scope, element, attrs, ngModel) {
                ngModel.$parsers.push(function (value) {
                    if (!angular.isNumber(value)) return value;
                    return String(value);
                });
                ngModel.$formatters.push(function (value) {
                    return parseInt(value, 10);
                });
            }
        };
    });

})(angular);