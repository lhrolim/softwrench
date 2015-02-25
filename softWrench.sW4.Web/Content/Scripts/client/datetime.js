var app = angular.module('sw_layout');

app.directive('dateTime', function ($timeout, formatService) {

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
//                console.log('no model, returning');
                return;
            }

            var showTime = parseBooleanValue(attrs.showTime);
            var originalAttribute = attrs.originalAttribute;
            var showDate = parseBooleanValue(attrs.showDate);
            var dateFormat = formatService.adjustDateFormatForPicker(attrs.dateFormat, showTime);
            attrs.language = (userLanguage != '') ? userLanguage : "en-US";
            var showMeridian = attrs.showAmpm == undefined ? undefined : attrs.showAmpm.toLowerCase() == "true";
            var istimeOnly = showTime && !showDate;
            var isReadOnly = attrs.readonly == undefined ? false : (attrs.readonly);

            datetimeclassHandler(istimeOnly);

            $timeout(function () {

                if (scope.fieldMetadata != undefined) {
                    var value = formatService.formatDate(ngModel.$modelValue, attrs.dateFormat);
                    ngModel.$setViewValue(value);
                    element.val(value);
                    if (originalAttribute != undefined) {
                        //this is useful on sections, like samelinepickers.html
                        scope.datamap[originalAttribute] = value;
                    }
                    else if (scope.datamap != undefined && scope.fieldMetadata != undefined) {
                        scope.datamap[scope.fieldMetadata.attribute] = value;
                    }
                    ngModel.$render();
                }

            });

            if (dateFormat != '' && dateFormat != undefined) {
                if (showTime) {
                    var futureOnly = (attrs.futureOnly != undefined && attrs.futureOnly.toLowerCase() == "true");
                    //                attrs.startDate = futureOnly ? '+0d' : -Infinity;

                    //if the date format starts with dd --> we don´t have the AM/PM thing, which is just an american thing where the dates starts with months
                    if (showMeridian == undefined) {
                        showMeridian = dateFormat.startsWith('MM');
                    }

                    element.datetimepicker({
                        format: dateFormat,
                        autoclose: true,
                        language: attrs.language,
                        todayBtn: false,
                        showMeridian: showMeridian,
                        startDate: attrs.startDate,
                        formatViewType: 'time',
                        startView: istimeOnly ? 1 : 2,
                        maxView: istimeOnly ? 1 : 3,
                        readonly: isReadOnly
                });
                }
                else {
                    var allowpast = parseBooleanValue(attrs.allowPast);
                    var allowfuture = parseBooleanValue(attrs.allowFuture);
                    var startDate = allowpast ? -Infinity : '+0d';
                    var endDate = allowfuture ? Infinity : '+0d';
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