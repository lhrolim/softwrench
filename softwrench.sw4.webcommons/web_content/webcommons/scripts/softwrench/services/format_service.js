﻿(function (angular) {
    "use strict";

    function formatService($filter, i18NService, dispatcherService) {

        //#region Utils

        var doFormatDate = function (value, dateFormat, forceConversion) {
            if (value == null) {
                return null;
            }

            if (angular.isString(value) && value.equalsAny("@now", "@currentdatetime", "@currentdate")) {
                return $filter('date')(new Date(), dateFormat);
            }

            if (forceConversion) {
                

                return $filter('date')(value, dateFormat);

                //this would be needed for applying the time formats
//                var date = new Date(value);
//                if (isNaN(date)) {
//                    return $filter('date')(value, dateFormat);
//                } else {
//                    return $filter('date')(date, dateFormat);
//                }
            }

            try {
                return $filter('date')(value, dateFormat);
            } catch (e) {
                value = new Date(value);
                return $filter('date')(value, dateFormat);
            }
        };

        var descriptionDataHandler = function (value, column) {
            if (value == null) {
                return "";
            }
            try {
                var unformatteddate = value.split('Logged Date:')[1];
                if (!nullOrUndef(unformatteddate)) {
                    var unformatteddatenospaces = unformatteddate.replace(/ /g, '');
                    var dateFormat = column.rendererParameters['format'];
                    var formatteddate = doFormatDate(unformatteddatenospaces, dateFormat, false);
                    value = value.replace(unformatteddatenospaces, formatteddate);
                }
            } catch (e) {
                return value;
            }
            return value;
        };

        //#endregion

        //#region Public methods

        function format (value, field, datamap) {
            if (field == undefined) {
                return value;
            }

            if (field.rendererParameters['formatter'] != undefined) {
                //If the formatter starts with an @ symbol
                if (field.rendererParameters['formatter'].startsWith("@")) {
                    var formatter = field.rendererParameters['formatter'];
                    formatter = formatter.substring(1); //Removes the leading '@' symbol
                    var serviceCall = formatter.split('.');
                    var serviceName = serviceCall[0];
                    var serviceMethod = serviceCall[1];

                    var fn = dispatcherService.loadService(serviceName, serviceMethod);

                    var parameters = {
                        'value': value,
                        'column': field,
                        'datamap': datamap
                    };
                    return fn(parameters);
                }
            }

            var dateFormat;
            if ((field.rendererType == "datetime" || field.rendererType == 'date' || field.rendererType == 'time')
                && (field.rendererParameters['formatter'] == null || field.rendererParameters['formatter'] == "datetime")) {
                if (value != null) {
                    dateFormat = field.rendererParameters['format'];
                    if (dateFormat == null) {
                        //TODO: make default client specific
                        dateFormat = "MM/dd/yyyy hh:mm";
                    }
                    return doFormatDate(value, dateFormat, false);
                }
            } else if (field.type == "ApplicationSection" && field.parameters['format']) {
                if (field.parameters['format'] != null && value != null) {
                    dateFormat = field.parameters['format'];
                    return doFormatDate(value, dateFormat, false);
                }
            } else if (field.rendererParameters != undefined && field.rendererParameters['formatter'] != null) {
                if (field.rendererParameters['formatter'] == 'numberToBoolean') {
                    value = value == 1 ? i18NService.get18nValue('general.yes', 'Yes') : i18NService.get18nValue('general.no', 'No');
                }
                else if (field.rendererParameters['formatter'] == 'numberToAbs') {
                    if (!isNaN(value)) {
                        value = Math.abs(value);
                    }
                }
                else if (field.rendererParameters['formatter'] == 'doubleToTime') {
                    if (value == null) {
                        return "";
                    }
                    //Converting to hh:mm
                    var time = value.toString();
                    if (time.length > 0 && time.indexOf('.') == -1) {
                        return value + " : 00";
                    }
                    var hours = time.split('.')[0];
                    var tempMins = time.split('.')[1];
                    hours = parseInt(hours);
                    var mins = Math.round(Math.round(parseFloat(0 + '.' + tempMins) * 60 * 100) / 100);
                    return (hours < 10 ? "0" + hours : hours) + " : " + (mins < 10 ? "0" + mins : "" + mins);
                }
                else if (field.rendererParameters['formatter'] == 'descriptionDataHandler') {
                    return descriptionDataHandler(value, field);
                }
            } else if (field.rendererParameters != undefined && field.rendererParameters['limit'] != null) {
                //format the word to only display the first n characters based on the limit 
                //once its formatted, also need to register a custom html but may not be here 
                if (typeof value != 'undefined') {
                    var val = value.toString();
                    var limit = field.rendererParameters['limit'];
                    var truncatedText = val.substring(0, field.rendererParameters['limit']);
                    if (val.length > limit) {
                        truncatedText += "...";
                    }
                    return truncatedText;
                }
            }

            return value;
        };


        function formatDate(value, dateFormat) {
            if (!dateFormat) {
                dateFormat = "MM/dd/yyyy hh:mm";
            }
            return doFormatDate(value, dateFormat, true);
        };

        function adjustDateFormatForAngular (dateFormat, showTime) {
            if (dateFormat == undefined || dateFormat == '') {
                //default ==> should be client specific
                return showTime ? "MM/dd/yyyy hh:mm" : "MM/dd/yyyy";
            } else {
                return dateFormat.trim();
            }
        };


        function adjustDateFormatForPicker (dateFormat, showTime) {
            /// <summary>
            ///  Bootstrap picker uses mm for month, and ii for minutes.
            ///  Angular, however, uses MM for month and hh mm for minutes.
            /// </summary>
            /// <param name="dateFormat"></param>
            /// <param name="showTime"></param>
            /// <returns type=""></returns>
            if (dateFormat == undefined || dateFormat == '') {
                //default ==> should be client specific
                return showTime ? "MM/DD/YYYY HH:mm" : "MM/DD/YYYY";
            } else {
                dateFormat = dateFormat.replace('dd', 'DD');
                dateFormat = dateFormat.replace('yyyy', 'YYYY');
                if (!showTime) {
                    //the format and the showtime flag are somehow conflitant, let´s adjust the format
                    dateFormat = dateFormat.replace('HH:mm', '');
                }
                return dateFormat.trim();
            }
        };

        function doContentStringConversion (datamap) {
            for (var record in datamap) {
                datamap[record] = datamap[record] == null ? null : datamap[record].toString();
            }

            return datamap;
        }

        //#endregion

        //#region Service Instance

        var service = {
            format: format,
            formatDate: formatDate,
            adjustDateFormatForAngular: adjustDateFormatForAngular,
            adjustDateFormatForPicker: adjustDateFormatForPicker,
            doContentStringConversion: doContentStringConversion
        };

        return service;


        //#endregion
    }

    //#region Service registration

    angular.module("webcommons_services").factory("formatService", ['$filter', 'i18NService', 'dispatcherService', formatService]);

    //#endregion

})(angular);