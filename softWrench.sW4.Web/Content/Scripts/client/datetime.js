var app = angular.module('sw_layout');

app.directive('dateTime', function ($timeout, formatService) {

    function parseBooleanValue(attrValue) {
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
            var showDate = parseBooleanValue(attrs.showDate);
            var dateFormat = formatService.adjustDateFormatForPicker(attrs.dateFormat, showTime);
            attrs.language = (userLanguage != '') ? userLanguage : "en-US";
            var showMeridian = parseBooleanValue(attrs.showMeridian);
            var istimeOnly = showTime && !showDate;

            datetimeclassHandler(istimeOnly);

            $timeout(function () {

                if (scope.fieldMetadata != undefined) {
                    var value = formatService.formatDate(ngModel.$modelValue, attrs.dateFormat);
                    ngModel.$setViewValue(value);
                    element.val(value);
                    if (scope.datamap != undefined && scope.fieldMetadata != undefined) {
                        scope.datamap[scope.fieldMetadata.attribute] = value;
                    }
                    ngModel.$render();
                }

            });

            if (dateFormat != '' && dateFormat != undefined) {
                if (istimeOnly) {
                    var futureOnly = (attrs.futureOnly != undefined && attrs.futureOnly.toLowerCase() == "true");
                    //                attrs.startDate = futureOnly ? '+0d' : -Infinity;

                    element.datetimepicker({
                        format: dateFormat,
                        autoclose: true,
                        language: attrs.language,
                        todayBtn: false,
                        showMeridian: false,
                        startDate: attrs.startDate,
                        formatViewType: 'time',
                        startView: 1,
                        maxView: 1
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
                        maxView: 3
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