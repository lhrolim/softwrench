var app = angular.module('sw_layout');

app.directive('dateTime', function ($timeout, formatService, expressionService) {

    function parseBooleanValue(attrValue) {
        return attrValue == undefined || attrValue == "" ? true : attrValue.toLowerCase() == "true";
    }

    function parseBooleanValueDefaultFalse(attrValue) {
        return attrValue == undefined || attrValue == "" ? true : attrValue.toLowerCase() == "true";
    }

    return {
        restrict: 'A',
        require: '?ngModel',
        link: function (scope, element, attrs, ngModel) {

            if (!ngModel) {
                return;
            }

            var showTime = parseBooleanValue(attrs.showTime);
            var showDate = parseBooleanValue(attrs.showDate);
            var dateFormat = formatService.adjustDateFormatForPicker(attrs.formatString, showTime);
            var angularDateFormat = formatService.adjustDateFormatForAngular(attrs.formatString, showTime);
            attrs.language = (userLanguage != '') ? userLanguage : 'en';
            var istimeOnly = showTime && !showDate;
            var datamap = scope.datamap;
            datetimeclassHandler(istimeOnly);

            $timeout(function () {
                if (scope.fieldMetadata != undefined) {
                    var value = formatService.formatDate(ngModel.$modelValue, angularDateFormat);
                    ngModel.$setViewValue(value);
                    element.val(value);
                    if (datamap != undefined && scope.fieldMetadata != undefined) {
                        datamap[scope.fieldMetadata.attribute] = value;
                    }
                    ngModel.$render();
                }
            });

            if (dateFormat != '' && dateFormat != undefined) {
                var allowfuture = parseBooleanValue(attrs.allowFuture);
                var allowpast = parseBooleanValue(attrs.allowPast);

                var startDate = allowpast ? false : new Date();
                var endDate = allowfuture ? false : new Date();
                var minStartDateExpression = attrs.minDateexpression;

                if (minStartDateExpression != null && minStartDateExpression != '') {
                    startDate = expressionService.evaluate(minStartDateExpression, datamap);
                    startDate = Date.parse(formatService.formatDate(startDate, attrs.dateFormat));
                    var variablesToWatch = expressionService.getVariablesForWatch(minStartDateExpression);
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
                    element.datetimepicker({
                        format: dateFormat,
                        locale: attrs.language,
                        maxDate: endDate,
                        minDate: startDate,
                        sideBySide: true,
                        showClose: showCloseButton,
                        toolbarPlacement: 'top',
                        useCurrent: false
                        //debug: true
                    });
                }, 0, false);
            }

            function datetimeclassHandler(timeOnly) {
                var datetime = $('.datetime-class', element.parent());
                var calendar = 'glyphicon glyphicon-calendar';
                var time = 'glyphicon glyphicon-time';
                datetime.removeClass(calendar);
                datetime.removeClass(time);
                var classToAdd = timeOnly ? time : calendar;
                datetime.addClass(classToAdd);
            }
        }
    };
});