(function (angular) {
    "use strict";

    angular.module("softwrench").directive("autosizeTextarea", ["$timeout", function ($timeout) {
        const directive = {
            link: function (scope, element, attrs) {
                var width = $(element).width();
                var fontSize = $(element).css('font-size');
                var fontFamily = $(element).css('font-family');

                function getNumberOfLines(string) {
                    if (!string) {
                        return 1;
                    }

                    const textWidth = getTextWidth(string, fontSize + ' ' + fontFamily);
                    const lines = Math.ceil(textWidth / width);

                    return lines;
                }

                function getTextWidth(text, font) {
                    // re-use canvas object for better performance
                    var canvas = getTextWidth.canvas || (getTextWidth.canvas = document.createElement("canvas"));
                    var context = canvas.getContext("2d");
                    context.font = font;
                    var metrics = context.measureText(text);
                    return metrics.width;
                }

                scope.$watch(attrs.ngModel, function (newValue) {
                    element[0].rows = getNumberOfLines(newValue);
                });

                scope.$on('sw:ionAutocomplete:viewValue', (event, value) => {
                    if (value.indexOf(element[0].value) > -1) {
                        $timeout(function () {
                            element[0].rows = getNumberOfLines(value);
                        });
                    }
                });

                scope.$on('sw:optionField:viewValue', (event, value) => {
                    $timeout(function() {
                        element[0].rows = getNumberOfLines(value);
                    });
                });
            }
        };

        return directive;
    }]);

})(angular);