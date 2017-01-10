﻿(function (angular) {
    "use strict";

angular.module('sw_components')
    .directive('dateTime', function ($timeout, formatService, expressionService) {
    "ngInject";

    function parseBooleanValue(attrValue) {
        return attrValue == undefined || attrValue == "" ? true : attrValue.toLowerCase() === "true";
    }

    function parseBooleanValueDefaultFalse(attrValue) {
        return attrValue == undefined || attrValue == "" ? true : attrValue.toLowerCase() === "true";
    }

    function updateView(ngModel, scope, element, datamap, angularDateFormat) {
        if (scope.fieldMetadata != undefined) {
            const value = formatService.formatDate(ngModel.$modelValue, angularDateFormat);
            ngModel.$setViewValue(value);
            element.val(value);
            if (datamap != undefined && scope.fieldMetadata != undefined) {
                datamap[scope.fieldMetadata.attribute] = value;
            }
            ngModel.$render();
        }
    }

    function datetimeclassHandler(timeOnly,element) {
        const datetime = $('.datetime-class', element.parent());
        const calendar = 'glyphicon glyphicon-calendar';
        const time = 'glyphicon glyphicon-time';
        datetime.removeClass(calendar);
        datetime.removeClass(time);
        const classToAdd = timeOnly ? time : calendar;
        datetime.addClass(classToAdd);
    }

    return {
        restrict: 'A',
        require: '?ngModel',
        link: function (scope, element, attrs, ngModel) {

            if (!ngModel) {
                return;
            }
            const showTime = parseBooleanValue(attrs.showTime);
            const showDate = parseBooleanValue(attrs.showDate);
            var dateFormat = formatService.adjustDateFormatForPicker(attrs.formatString, showTime);
            var angularDateFormat = formatService.adjustDateFormatForAngular(attrs.formatString, showTime);
            attrs.language = (userLanguage !== '') ? userLanguage : 'en';
            const istimeOnly = showTime && !showDate;
            var datamap = scope.datamap;
            datetimeclassHandler(istimeOnly,element);

            scope.$watch(function () {
                return ngModel.$modelValue;
            }, function (newValue) {
                //let´s make sure that the view keeps getting update if the model changes via an api call
                updateView(ngModel, scope, element, datamap, angularDateFormat);
            });

            $timeout(function () {
                updateView(ngModel, scope, element, datamap, angularDateFormat);
            });

            if (dateFormat !== '' && dateFormat != undefined) {
                const allowfuture = parseBooleanValue(attrs.allowFuture);
                const allowpast = parseBooleanValue(attrs.allowPast);
                const position = !!attrs.position ? attrs.position: "bottom";
                var startDate = allowpast ? false : new Date();
                var endDate = allowfuture ? false : new Date();
                var minStartDateExpression = attrs.minDateexpression;

                if (minStartDateExpression != null && minStartDateExpression != '') {
                    startDate = expressionService.evaluate(minStartDateExpression, datamap);
                    startDate = Date.parse(formatService.formatDate(startDate, attrs.dateFormat));
                    const variablesToWatch = expressionService.getVariablesForWatch(minStartDateExpression);
                    scope.$watchCollection(variablesToWatch, function (newVal, oldVal) {
                        if (newVal != oldVal) {
                            startDate = expressionService.evaluate(minStartDateExpression, datamap);
                            startDate = formatService.formatDate(startDate, attrs.dateFormat);
                            element.data('datetimepicker').startDate = Date.parse(startDate);
                        }
                    });
                }

                //only show close button if both date and time are shown
                var showCloseButton = false;
                if (dateFormat.indexOf("/") > -1 && dateFormat.indexOf(":") > -1) {
                    showCloseButton = true;
                }

                $timeout(function () {
                    //timeout to avoid $digest is already in progress exception... using false keyword postergates this to next digest loop
                    // element.datetimepicker({
                    const elementToUse = !attrs.attachDatepickerToParent ? element : element.parent();
                    const datetimepicker = elementToUse.datetimepicker({
                        format: dateFormat,
                        locale: attrs.language,
                        maxDate: endDate,
                        minDate: startDate,
                        sideBySide: true,
                        showClose: showCloseButton,
                        toolbarPlacement: 'top',
                        useCurrent: false,
                        widgetPositioning: {
                            vertical: position
                        }
                        //debug: true
                    });
                    if (isIE()) {
                        //https://controltechnologysolutions.atlassian.net/browse/SWWEB-2198
                        datetimepicker.on("dp.change", function (e) {
                            scope.$apply(function() {
                                ngModel.$modelValue = e.formattedDate;
                                const value = formatService.formatDate(ngModel.$modelValue, angularDateFormat);
                                if (datamap != undefined && scope.fieldMetadata != undefined) {
                                    datamap[scope.fieldMetadata.attribute] = value;
                                }
//                                updateView(ngModel, scope, element, datamap, angularDateFormat);
                            });

                        });
                    }
                }, 0, false);
            }

          
        }
    };
});

})(angular);