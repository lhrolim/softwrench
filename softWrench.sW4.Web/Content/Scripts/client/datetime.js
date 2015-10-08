var app = angular.module('sw_layout');

app.directive('dateTime', function ($timeout, formatService, expressionService) {

    function parseBooleanValue(attrValue) {
        return attrValue == undefined || attrValue == "" ? true : attrValue.toLowerCase() == "true";
    }

    function parseBooleanValueDefaultFalse(attrValue) {
        return attrValue == undefined || attrValue == "" ? true : attrValue.toLowerCase() == "true";
    }

    function evaluateMinStartDate(attrs, scope, element) {
        var datamap = scope.datamap;
        var allowpast = parseBooleanValue(attrs.allowPast);
        var minStartDateExpression = attrs.mindateexpression;
        if (minStartDateExpression != null) {
            var startdate = expressionService.evaluate(minStartDateExpression, datamap);
            var variablesToWatch = expressionService.getVariablesForWatch(minStartDateExpression);
            $scope.$watchCollection(variablesToWatch, function (newVal, oldVal) {
                if (newVal != oldVal) {
                    element.datepicker().startdate = expressionService.evaluate(minStartDateExpression, datamap);
                }
            });
            return startdate;
        } else {
            return allowpast ? -Infinity : '+0d';
        }
    }


    return {
        restrict: 'A',
        require: '?ngModel',
        link: function (scope, element, attrs, ngModel) {


            if (!ngModel) {
                //                console.log('no model, returning');
                return;
            }

            var showTime = parseBooleanValue(attrs.showTime);
            var showIgnoreTime = attrs.showIgnoretime==="true";
            var originalAttribute = attrs.originalAttribute;
            var showDate = parseBooleanValue(attrs.showDate);
            var dateFormat = formatService.adjustDateFormatForPicker(attrs.dateFormat, showTime);
            if (!attrs.language || attrs.language == "") {
                attrs.language = (userLanguage != '') ? userLanguage : "en-US";
            }
            var showMeridian = attrs.showAmpm == undefined ? undefined : attrs.showAmpm.toLowerCase() == "true";
            var istimeOnly = showTime && !showDate;
            var isReadOnly = attrs.readonly == undefined ? false : (attrs.readonly);
            var datamap = scope.datamap;
            datetimeclassHandler(istimeOnly);

            $timeout(function () {

                if (scope.fieldMetadata != undefined) {
                    var value = formatService.formatDate(ngModel.$modelValue, attrs.dateFormat);
                    ngModel.$setViewValue(value);
                    element.val(value);
                    if (originalAttribute != undefined) {
                        //this is useful on sections, like samelinepickers.html
                        datamap[originalAttribute] = value;
                    }
                    else if (datamap != undefined && scope.fieldMetadata != undefined) {
                        datamap[scope.fieldMetadata.attribute] = value;
                    }
                    ngModel.$render();
                }

            });

            if (dateFormat != '' && dateFormat != undefined) {

                var allowfuture = parseBooleanValue(attrs.allowFuture);
                var allowpast = parseBooleanValue(attrs.allowPast);
                var startDate = allowpast ? -Infinity : '+0d';
                var endDate = allowfuture ? Infinity : '+0d';
                var minStartDateExpression = attrs.minDateexpression;

                if (showTime) {
                    var futureOnly = (attrs.futureOnly != undefined && attrs.futureOnly.toLowerCase() == "true");
                    //                attrs.startDate = futureOnly ? '+0d' : -Infinity;

                    //if the date format starts with dd --> we don´t have the AM/PM thing, which is just an american thing where the dates starts with months
                    if (showMeridian == undefined) {
                        showMeridian = dateFormat.startsWith('MM');
                    }

                    if (minStartDateExpression != null && minStartDateExpression != "") {
                        startDate = expressionService.evaluate(minStartDateExpression, datamap);
                        startDate = formatService.formatDate(startDate, attrs.dateFormat);
                        variablesToWatch = expressionService.getVariablesForWatch(minStartDateExpression);
                        scope.$watchCollection(variablesToWatch, function (newVal, oldVal) {
                            if (newVal != oldVal) {
                                startDate = expressionService.evaluate(minStartDateExpression, datamap);
                                startDate = formatService.formatDate(startDate, attrs.dateFormat);
                                element.data('datetimepicker').startDate= Date.parse(startDate);
                            }
                        });
                    }

                    element.datetimepicker({
                        format: dateFormat,
                        autoclose: true,
                        language: attrs.language,
                        todayBtn: false,
                        showMeridian: showMeridian,
                        startDate: startDate,
                        formatViewType: 'time',
                        startView: istimeOnly ? 1 : 2,
                        maxView: istimeOnly ? 1 : 3,
                        readonly: isReadOnly,
                        ignoretimeBtn: showIgnoreTime,
                        forceParse :!showIgnoreTime


                    });
                }
                else {

                    
                    if (minStartDateExpression != null && minStartDateExpression!="") {
                        startDate = expressionService.evaluate(minStartDateExpression, datamap);
                        startDate = formatService.formatDate(startDate, attrs.dateFormat);
                        var variablesToWatch = expressionService.getVariablesForWatch(minStartDateExpression);
                        scope.$watchCollection(variablesToWatch, function (newVal, oldVal) {
                            if (newVal != oldVal) {
                                startDate = expressionService.evaluate(minStartDateExpression, datamap);
                                startDate = formatService.formatDate(startDate, attrs.dateFormat);
                                element.data('datepicker').startDate = Date.parse(startDate);
                            }
                        });
                    }

                    element.datepicker({
                        startDate: startDate,
                        endDate: endDate,
                        format: dateFormat,
                        autoclose: true,
                        language: attrs.language,
                        maxView: 3,
                        showMeridian: false,
                        readonly: isReadOnly
                    });
                }
            }

            /*
            element.bind('blur keyup change', function () {
                scope.$apply(read);
            });
            */
            function datetimeclassHandler(timeOnly) {
                var datetime = $('.datetime-class').last();
                var calendar = 'glyphicon glyphicon-calendar';
                var time = 'glyphicon glyphicon-time';
                datetime.removeClass(calendar);
                datetime.removeClass(time);
                var classToAdd = timeOnly ? time : calendar;
                datetime.addClass(classToAdd);
            }
            /*
            function read() {
                if (scope.datamap) {
                    ngModel.$setViewValue(element.val());
                }
            }
            */


        }
    };
});