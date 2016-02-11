(function (angular, $) {
    // "use strict";
    // [IMPORTANT!] Do not go back to strict mode, it causes a production error:
    // 'Uncaught SyntaxError: In strict mode code, functions can only be declared at top level or immediately within another function' 
    // for the `watchForStartDate` function

    var app = angular.module("sw_layout");

    app.directive("dateTime", ["$timeout", "formatService", "expressionService", "$q", function ($timeout, formatService, expressionService, $q) {

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
                // no model, not required to do anything fancy
                if (!ngModel) {
                    return;
                }
                // block usage of ng-enter: use date-enter instead
                if (!!attrs.ngEnter) {
                    throw new Error("ng-enter directive is not supported. Use date-enter instead.");
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
                        var localStartDate = expressionService.evaluate(minStartDateExpression, datamap);
                        localStartDate = formatService.formatDate(localStartDate, originalDateFormat);
                        var variablesToWatch = expressionService.getVariablesForWatch(minStartDateExpression);
                        scope.$watchCollection(variablesToWatch, function (newVal, oldVal) {
                            if (newVal !== oldVal) {
                                localStartDate = expressionService.evaluate(minStartDateExpression, datamap);
                                localStartDate = formatService.formatDate(localStartDate, originalDateFormat);
                                var datePicker = element.data(picker);
                                startDate = Date.parse(localStartDate);
                                datePicker.startDate = startDate;
                                datePicker.initialDate = datePicker.startDate;
                            }
                        });
                        return Date.parse(localStartDate);
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
                     * @returns Promise
                     */
                    function renderValue(value) {
                        var deferred = $q.defer();
                        scope.$apply(function () {
                            ngModel.$setViewValue(value);
                            ngModel.$render();
                            deferred.resolve(value);
                        });
                        return deferred.promise;
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
                     * Validates the date (both the time and format) and returns a value that can be rendered:
                     * - if format is wrong and has stardate render formatted startdate
                     * - if format is wrong and has no startdate render empty string
                     * - if format is right but date is invalid render formatted start date
                     * 
                     * @param String value 
                     * @returns String 
                     */
                    function getRenderableDateValue(value) {
                        try {
                            var dateFromUser = $.fn.datetimepicker.DPGlobal.parseDate(value, $.fn.datepicker.DPGlobal.parseFormat(dateFormat), attrs.language);
                            // attempt a regular parse just in case:
                            // this may be necessary when the user causes the parse of an already formatted date (e.g. multiple clicking enter)
                            if (isInvalidValidDate(dateFromUser)) {
                                dateFromUser = Date.parse(value);
                            }
                            var dateToUse = isInvalidValidDate(dateFromUser) ? startDate : dateFromUser;
                            return formatService.formatDate(dateToUse, originalDateFormat);
                        } catch (e) {
                            return angular.isDate(startDate) ? formatService.formatDate(startDate, originalDateFormat) : "";
                        }
                    }

                    /**
                     * Handles the blur event on a datepicker field.
                     * It's useful for when the user manually writes the date (doesn't trigger DateTimePicker events nor JQuery.Event<change>).
                     * Renders the value obtained from getRenderableDateValue(<input_value>).
                     * 
                     * @param JQuery.Event<Blur> event 
                     */
                    function blurHandler(event) {
                        var inputValue = $(this).val();
                        if (!inputValue) return;
                        var dateToRender = getRenderableDateValue(inputValue);
                        renderValue(dateToRender);
                    }

                    /**
                     * Handles the pressing of the 'enter' key on datepicker input.
                     * Renders the value obtained from getRenderableDateValue(<input_value>) then executes any ng-enter expression.
                     * Cancels the event.
                     * 
                     * @param JQuery.Event<keypress> event 
                     */
                    function enterHandler(event) {
                        if (event.which !== 13) return;
                        var value = $(this).val();
                        if (!value) return;
                        // render value
                        var date = getRenderableDateValue(value);
                        var promise = renderValue(date);
                        // schedule to execute date-enter callback after rendering the value and cancelling the 'enter' event 
                        if (!!attrs.dateEnter) {
                            promise.then(function () {
                                scope.$eval(attrs.dateEnter);
                            });
                        }
                        // cancelling event
                        event.preventDefault();
                        event.stopPropagation();
                        event.stopImmediatePropagation();
                    }

                    var initialDate;
                    if (showTime) {
                        // var futureOnly = (attrs.futureOnly != undefined && attrs.futureOnly.toLowerCase() === "true");
                        // attrs.startDate = futureOnly ? '+0d' : -Infinity;

                        //if the date format starts with dd --> we don´t have the AM/PM thing, which is just an american thing where the dates starts with months
                        if (!showMeridian) {
                            showMeridian = dateFormat.startsWith("MM");
                        }

                        if (!!attrs.minDateexpression) startDate = watchForStartDate("datetimepicker");
                        initialDate = startDate === defaultStartDate ? null : startDate;

                        element.datetimepicker({
                            format: dateFormat,
                            autoclose: true,
                            language: attrs.language,
                            todayBtn: false,
                            showMeridian: showMeridian,
                            startDate: startDate,
                            initialDate: initialDate,
                            formatViewType: "time",
                            startView: istimeOnly ? 1 : 2,
                            maxView: istimeOnly ? 1 : 3,
                            readonly: isReadOnly,
                            ignoretimeBtn: showIgnoreTime,
                            forceParse: !showIgnoreTime
                        }).on("blur", blurHandler).on("changeDate", changeDateHandler).keypress(enterHandler);

                    } else {

                        if (!!attrs.minDateexpression) startDate = watchForStartDate("datepicker");
                        initialDate = startDate === defaultStartDate ? null : startDate;

                        element.datepicker({
                            initialDate: initialDate,
                            startDate: startDate,
                            endDate: endDate,
                            format: dateFormat,
                            autoclose: true,
                            language: attrs.language,
                            maxView: 3,
                            showMeridian: false,
                            readonly: isReadOnly
                        }).on("blur", blurHandler).on("changeDate", changeDateHandler).keypress(enterHandler);
                    }
                }
            }
        };
    }]);

})(angular, jQuery);