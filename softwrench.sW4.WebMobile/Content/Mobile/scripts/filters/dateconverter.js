
(function () {
    "use strict";

    angular.module("softwrench").directive("dateconverter", ["formatService", function (formatService) {
        var directive = {
            restrict: 'A',
            require: '?ngModel',
            link: function (scope, element, attrs, modelCtrl) {


                modelCtrl.$parsers.push(function (inputValue) {
                    if (inputValue instanceof Date) {
                        return formatService.formatDate(inputValue, scope.field.rendererParameters['format']);
                    }
                    return inputValue;
                });

                modelCtrl.$formatters.push(function(modelValue) {
                    return new Date(modelValue);
                });



            }
        };

        return directive;

    }]);

})();


