(function (angular) {
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

                scope.dateHandler = (function () {
                    function nowWhenNotAllowed(field, parameter) {
                        var rendererParameters = field.rendererParameters;
                        return (rendererParameters.hasOwnProperty(parameter)
                            && (rendererParameters[parameter] === "false" || rendererParameters[parameter] === false))
                                ? new Date()
                                : null;
                    }
                    function getMaxDateValue(field) {
                        return nowWhenNotAllowed(field, "allowfuture");
                    }
                    function getMinDateValue(field) {
                        return nowWhenNotAllowed(field, "allowpast");
                    }
                    return {
                        getMaxDateValue: getMaxDateValue,
                        getMinDateValue: getMinDateValue
                    };
                })();


            }
        };

        return directive;

    }]);

})(angular);


