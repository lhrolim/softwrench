(function (angular) {
    "use strict";

    function formatService($filter, i18NService, dispatcherService, contextService) {
        //#region Utils

        var doFormatDate = function (value, dateFormat, forceConversion, contextService) {
            if (value == null) {
                return null;
            }

            if (angular.isString(value) && value.equalsAny("@now", "@currentdatetime", "@currentdate")) {
                return $filter('date')(new Date(), dateFormat);
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

        function format(value, field, datamap) {
            if (field == undefined || value == null) {
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
                else if (field.rendererParameters['formatter'] === "lower") {
                    return value.toLowerCase().trim();
                }
                else if (field.rendererParameters['formatter'] === "upper") {
                    return value.toUpperCase().trim();
                }

            }

            var dateFormat;
            if ((field.rendererType && field.rendererType.equalsAny("datetime", 'date','time'))
                && (field.rendererParameters['formatter'] == null || field.rendererParameters['formatter'] === "datetime")) {
                if (value != null) {
                    dateFormat = field.rendererParameters['format'];
                    if (dateFormat == null) {
                        //TODO: make default client specific
                        dateFormat = contextService.retrieveFromContext('dateTimeFormat');;
                    }
                    return doFormatDate(value, dateFormat, false);
                }
            } else if (field.type === "ApplicationSection" && field.parameters['format']) {
                if (field.parameters['format'] != null && value != null) {
                    dateFormat = field.parameters['format'];
                    return doFormatDate(value, dateFormat, false);
                }
            } else if (field.rendererParameters != undefined && field.rendererParameters['formatter'] != null) {
                if (field.rendererParameters['formatter'] === 'numberToBoolean') {
                    value = value == 1 ? i18NService.get18nValue('general.yes', 'Yes') : i18NService.get18nValue('general.no', 'No');
                }

                if (field.rendererParameters['formatter'] === 'booltostring') {
                    value = value === true ? "true" : "false";
                }

                else if (field.rendererParameters['formatter'] === 'numberToAbs') {
                    if (!isNaN(value)) {
                        value = Math.abs(value);
                    }
                }
                else if (field.rendererParameters['formatter'] === 'doubleToTime') {
                    if (value == null) {
                        return "";
                    }
                    //Converting to hh:mm
                    var time = value.toString();
                    if (time.length > 0 && time.indexOf('.') === -1) {
                        return value + " : 00";
                    }
                    var hours = time.split('.')[0];
                    var tempMins = time.split('.')[1];
                    hours = parseInt(hours);
                    var mins = Math.round(Math.round(parseFloat(0 + '.' + tempMins) * 60 * 100) / 100);
                    return (hours < 10 ? "0" + hours : hours) + " : " + (mins < 10 ? "0" + mins : "" + mins);
                }
                else if (field.rendererParameters['formatter'] === 'doubleToTimeExt') {
                    if (value == null) {
                        return "";
                    }
                    // Convert hours to milliseconds
                    var milliseconds = value * 3600000;
                    var time = new Date(milliseconds);
                    var days = Math.floor(value / 24);
                    var hours = time.getUTCHours();
                    var mins = time.getUTCMinutes();
                    var secs = time.getUTCSeconds();

                    return (days < 10 ? "0" + days : days) + " : " + (hours < 10 ? "0" + hours : hours) + " : " + (mins < 10 ? "0" + mins : "" + mins) + " : " + (secs < 10 ? "0" + secs : "" + secs);
                }
                else if (field.rendererParameters['formatter'] == 'descriptionDataHandler') {
                    return descriptionDataHandler(value, field);
                }
                else if (field.rendererParameters['formatter'] == 'money') {
                    return $filter('currency')(value, '$', 2);
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
                dateFormat = contextService.retrieveFromContext('dateTimeFormat');
            }
            return doFormatDate(value, dateFormat, true);
        };

        function adjustDateFormatForAngular(dateFormat, showTime) {
            if (!dateFormat) {
                var globalDateTimeFormat = contextService.retrieveFromContext('dateTimeFormat');
                //default ==> should be client specific
                return showTime ? globalDateTimeFormat : globalDateTimeFormat.replace('hh:mm', '');
            } else {
                return dateFormat.trim();
            }
        };


        function adjustDateFormatForPicker(dateFormat, showTime, showMinutes=true) {
            /// <summary>
            ///  Bootstrap picker uses mm for month, and ii for minutes.
            ///  Angular, however, uses MM for month and hh mm for minutes.
            /// </summary>
            /// <param name="dateFormat"></param>
            /// <param name="showTime"></param>
            /// <returns type=""></returns>
            if (!dateFormat) {
                dateFormat = contextService.retrieveFromContext('dateTimeFormat');
                dateFormat = dateFormat.replace('dd', 'DD');
                dateFormat = dateFormat.replace('yyyy', 'YYYY');
//                dateFormat = dateFormat.replace('hh', 'HH');
                //default ==> should be client specific
                if (!showTime) {
                    dateFormat = dateFormat.replace('HH:mm', '');
                    dateFormat =dateFormat.replace('hh:mm', '');
                }
                if (!showMinutes) {
                    dateFormat = dateFormat.replace(':mm', ':00');
                }
                return dateFormat;
            } else {
                dateFormat = dateFormat.replace('dd', 'DD');
                dateFormat = dateFormat.replace('yyyy', 'YYYY');
                if (!showTime) {
                    //the format and the showtime flag are somehow conflitant, let´s adjust the format
                    dateFormat = dateFormat.replace('HH:mm', '');
                    dateFormat = dateFormat.replace('hh:mm', '');
                }
                if (!showMinutes) {
                    dateFormat = dateFormat.replace(':mm', ':00');
                }
                return dateFormat.trim();
            }
        };

        function doContentStringConversion(datamap) {
            angular.forEach(datamap, function (value, index) {
                datamap[index] = value == null
                   ? null
                   : angular.isArray(value)
                       ? value
                       : value.toString();
            });

            return datamap;
        }

        function isChecked(content) {

            switch (typeof content) {
                case "boolean":
                    return content;
                case "number":
                    return content === 1;
                case "string":
                    return content.equalsIc("y") || content.equalsIc("yes") || content.equalsIc("true") || content.equalsIc("1");
            }

            return false;
        }

        //#endregion

        //#region Service Instance

        const service = {
            format,
            formatDate,
            adjustDateFormatForAngular,
            adjustDateFormatForPicker,
            doContentStringConversion,
            isChecked
        };

        return service;


        //#endregion
    }

    //#region Service registration

    angular.module("webcommons_services").service("formatService", ['$filter', 'i18NService', 'dispatcherService', 'contextService', formatService]);

    //#endregion

})(angular);