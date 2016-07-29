(function (angular) {
    "use strict";

    angular.module("webcommons_services").directive("floatConverter", [function () {
        const directive = {
            restrict: "A",
            transclude: false,
            replace: false,
            require: "ngModel",
            // scope: true,
            link: function (scope, element, attrs, ngModel) {
                ngModel.$parsers.push(viewValue => {
                    const modelValue = parseFloat(viewValue);
                    return !angular.isNumber(modelValue) || isNaN(modelValue) ? 0 : modelValue;
                });

                ngModel.$formatters.push(modelValue => element[0].type === "number" ? modelValue : String(modelValue));
            }
        };

        return directive;
    }]);

})(angular);