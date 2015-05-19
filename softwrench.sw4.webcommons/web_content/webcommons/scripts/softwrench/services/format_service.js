var app = angular.module('sw_layout');

app.factory('formatService', function ($filter, i18NService, dispatcherService) {

    var doFormatDate = function (value, dateFormat, forceConversion) {
        if (value == null) {
            return null;
        }

        if (value == "@currentdatetime" || value == "@currentdate") {
            return $filter('date')(new Date(), dateFormat);
        }

        if (forceConversion) {
            //this would be needed for applying the time formats
            var date = new Date(value);
            if (isNaN(date)) {
                return $filter('date')(value, dateFormat);
            } else {
                return $filter('date')(date, dateFormat);
            }
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

    return {
        format: function (value, field, datamap) {
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
            if (field.rendererType == "datetime" || field.rendererType == 'date' || field.rendererType == 'time' || field.rendererParameters['formatter'] == "datetime") {
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
        },

        formatDate: function (value, dateFormat) {
            return doFormatDate(value, dateFormat, true);
        },

        adjustDateFormatForAngular: function (dateFormat, showTime) {
            if (dateFormat == undefined || dateFormat == '') {
                //default ==> should be client specific
                return showTime ? "MM/dd/yyyy hh:mm" : "MM/dd/yyyy";
            } else {
                return dateFormat.trim();
            }
        },


        adjustDateFormatForPicker: function (dateFormat, showTime) {
            /// <summary>
            ///  Bootstrap picker uses mm for month, and ii for minutes.
            ///  Angular, however, uses MM for month and hh mm for minutes.
            /// </summary>
            /// <param name="dateFormat"></param>
            /// <param name="showTime"></param>
            /// <returns type=""></returns>
            if (dateFormat == undefined || dateFormat == '') {
                //default ==> should be client specific
                return showTime ? "mm/dd/yyyy hh:ii" : "mm/dd/yyyy";
            } else {
                dateFormat = dateFormat.replace('MM', 'mm');
                dateFormat = dateFormat.replace(':mm', ':ii');
                dateFormat = dateFormat.replace('HH', 'hh');
                if (!showTime) {
                    //the format and the showtime flag are somehow conflitant, let´s adjust the format
                    dateFormat = dateFormat.replace('hh:ii', '');
                }
                return dateFormat.trim();
            }
        },

        doContentStringConversion: function (datamap) {
            for (var record in datamap) {
                datamap[record] = datamap[record] == null ? null : datamap[record].toString();
            }

            return datamap;
        }
    };

});