(function (angular, $) {
    "use strict";

var app = angular.module("sw_layout");

app.directive("dateTime", function ($timeout, formatService, expressionService) {

    function parseBooleanValue(attrValue) {
        return !attrValue ? true : attrValue.toLowerCase() === "true";
    }

    function datetimeclassHandler(timeOnly) {
        var datetime = $(".datetime-class").last();
        var calendar = "glyphicon glyphicon-calendar";
        var time = "glyphicon glyphicon-time";
        datetime.removeClass(calendar);
        datetime.removeClass(time);
        var classToAdd = timeOnly ? time : calendar;
        datetime.addClass(classToAdd);
    }

    // 01/jan/1970 00:00:00
    var defaultStartDate = new Date(1970, 0, 1, 0, 0, 0);

    return {
        restrict: "A",
        require: "?ngModel",
        link: function (scope, element, attrs, ngModel) {

            if (!ngModel) {
                return;
            }

            var showTime = parseBooleanValue(attrs.showTime);
            var showIgnoreTime = attrs.showIgnoretime === "true";
            var originalAttribute = attrs.originalAttribute;
            var showDate = parseBooleanValue(attrs.showDate);
            var dateFormat = formatService.adjustDateFormatForPicker(attrs.dateFormat, showTime);
            var originalDateFormat = attrs.dateFormat;
            if (!attrs.language) {
                attrs.language = userLanguage || "en-US";
            }
            var showMeridian = attrs.showAmpm == undefined ? undefined : attrs.showAmpm.toLowerCase() === "true";
            var istimeOnly = showTime && !showDate;
            var isReadOnly = attrs.readonly == undefined ? false : (attrs.readonly);
            var datamap = scope.datamap;
            datetimeclassHandler(istimeOnly);

            $timeout(function () {
                if (!!scope.fieldMetadata) {
                    var value = formatService.formatDate(ngModel.$modelValue, attrs.dateFormat);
                    ngModel.$setViewValue(value);
                    element.val(value);
                    if (!!originalAttribute) {
                        //this is useful on sections, like samelinepickers.html
                        datamap[originalAttribute] = value;
                    } else if (!!datamap && !!scope.fieldMetadata) {
                        datamap[scope.fieldMetadata.attribute] = value;
                    }
                    ngModel.$render();
                }
            });

            if (!!dateFormat) {
                var allowfuture = parseBooleanValue(attrs.allowFuture);
                var allowpast = parseBooleanValue(attrs.allowPast);
                var startDate = allowpast ? defaultStartDate : "+0d";
                var endDate = allowfuture ? Infinity : "+0d";
                var minStartDateExpression = attrs.minDateexpression;

                // watch on variables that need to cause a change in the picker's 'startDate' property
                function watchForStartDate(picker) {
                    startDate = expressionService.evaluate(minStartDateExpression, datamap);
                    startDate = formatService.formatDate(startDate, dateFormat);
                    var variablesToWatch = expressionService.getVariablesForWatch(minStartDateExpression);
                    scope.$watchCollection(variablesToWatch, function (newVal, oldVal) {
                        if (newVal !== oldVal) {
                            startDate = expressionService.evaluate(minStartDateExpression, datamap);
                            startDate = formatService.formatDate(startDate, dateFormat);
                            element.data(picker).startDate = Date.parse(startDate);
                        }
                    });
                }

                /**
                 * Checks if a given date is before the startDate.
                 * 
                 * @param Date date 
                 * @returns Boolean 
                 */
                function isInvalidValidDate(date) {
                    return angular.isDate(startDate) && date.getTime() < startDate.getTime();
                }

                /**
                 * Sets the ngModelController's $viewValue and $render's it.
                 *
                 * @param {} value 
                 */
                function renderValue(value) {
                    $timeout(function () {
                        ngModel.$setViewValue(value);
                        ngModel.$render();
                    });
                }

                /**
                 * Intercepts the date the user set and checks if it's an invalid date. 
                 * In this case renders formattted starDate.
                 * 
                 * @param Bootstrap.DateTimePicker.Event<changeDate> event 
                 */
                function changeDateHandler(event) {
                    var date = event.date;
                    if (!date) return;
                    if (isInvalidValidDate(date)) renderValue(formatService.formatDate(startDate, originalDateFormat));
                };

                /**
                 * Handles the blur event on a datepicker field.
                 * It's useful for when the user manually writes the date (doesn't trigger DateTimePicker events nor JQuery.Event<change>).
                 * Validates the date (both the time and format):
                 * - if format is wrong and has stardate render formatted startdate
                 * - if format is wrong and has no startdate render empty string
                 * - if format is right but date is invalid render formatted start date
                 * 
                 * @param JQuery.Event<Blur> event 
                 */
                function blurHandler(event) {
                    var inputValue = $(this).val();
                    if (!inputValue) return;
                    try {
                        var dateFromUser = $.fn.datetimepicker.DPGlobal.parseDate(inputValue, $.fn.datepicker.DPGlobal.parseFormat(dateFormat), attrs.language);
                        if (isInvalidValidDate(dateFromUser)) renderValue(formatService.formatDate(startDate, originalDateFormat));
                    } catch (e) {
                        angular.isDate(startDate) ? renderValue(formatService.formatDate(startDate, originalDateFormat)) : renderValue("");
                    }
                }

                if (showTime) {
                    // var futureOnly = (attrs.futureOnly != undefined && attrs.futureOnly.toLowerCase() === "true");
                    // attrs.startDate = futureOnly ? '+0d' : -Infinity;

                    //if the date format starts with dd --> we don´t have the AM/PM thing, which is just an american thing where the dates starts with months
                    if (!showMeridian) {
                        showMeridian = dateFormat.startsWith("MM");
                    }

                    if (!!attrs.minDateexpression) watchForStartDate("datetimepicker");
                    
                    element.datetimepicker({
                        format: dateFormat,
                        autoclose: true,
                        language: attrs.language,
                        todayBtn: false,
                        showMeridian: showMeridian,
                        startDate: startDate, 
                        formatViewType: "time",
                        startView: istimeOnly ? 1 : 2,
                        maxView: istimeOnly ? 1 : 3,
                        readonly: isReadOnly,
                        ignoretimeBtn: showIgnoreTime,
                        forceParse :!showIgnoreTime
                    }).on("blur", blurHandler).on("changeDate", changeDateHandler);

                } else {

                    if (!!attrs.minDateexpression) watchForStartDate("datepicker");

                    element.datepicker({
                        startDate: startDate,
                        endDate: endDate,
                        format: dateFormat,
                        autoclose: true,
                        language: attrs.language,
                        maxView: 3,
                        showMeridian: false,
                        readonly: isReadOnly
                    }).on("blur", blurHandler).on("changeDate", changeDateHandler);
                }
            }
        }
    };
});

})(angular, jQuery);