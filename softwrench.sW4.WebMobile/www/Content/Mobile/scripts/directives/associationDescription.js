(function (angular) {
    "use strict";

    angular.module("softwrench").directive("associationDescription", ["$parse",function ($parse) {
        var directive = {
            restrict: 'A',
            require: '?ngModel',
            link: function (scope, element, attrs, modelCtrl) {

                modelCtrl.$formatters.push(function (modelValue) {
                    var field = scope.field;
                    if ("true" !== field.hideDescription) {
                        return modelValue + " - " + scope.datamap[field.associationKey + "." + field.labelFields[0]];
                    }
                    return modelValue;
                });

            }
        };

        return directive;

    }]);

})(angular);