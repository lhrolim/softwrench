var app = angular.module('sw_layout');

app.factory('formatService', function ($filter, i18NService) {

    var doFormatDate = function (value, dateFormat, forceConversion) {
        if (value == null) {
            return null;
        }

        if (value === "@currentdatetime" || value === "@currentdate") {
            return $filter('date')(new Date(), dateFormat);
        }

        if (forceConversion) {
            //this would be needed for applying the time formats
            var date = Date.parse(value);
            if (date== null || isNaN(date)) {
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
        format: function (value, column) {
            if (column == undefined) {
                return value;
            }
            var dateFormat;
            if (column.rendererType == "datetime") {
                if (column.rendererParameters['format'] != null && value != null) {
                    dateFormat = column.rendererParameters['format'];
                    return doFormatDate(value, dateFormat, false);
                }
            } else if (column.type == "ApplicationSection" && column.parameters['format']) {
                if (column.parameters['format'] != null && value != null) {
                    dateFormat = column.parameters['format'];
                    return doFormatDate(value, dateFormat, false);
                }
            } else if (column.rendererParameters != undefined && column.rendererParameters['formatter'] != null) {
                if (column.rendererParameters['formatter'] == 'numberToBoolean') {
                    value = value == 1 ? i18NService.get18nValue('general.yes', 'Yes') : i18NService.get18nValue('general.no', 'No');
                }
                else if (column.rendererParameters['formatter'] == 'doubleToTime') {
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
                else if (column.rendererParameters['formatter'] == 'descriptionDataHandler') {
                    return descriptionDataHandler(value, column);
                }
            }

            return value;
        },

        formatDate: function (value, dateFormat) {
            return doFormatDate(value, dateFormat, true);
        },

        adjustDateFormatForPicker: function (dateFormat, showTime) {
            if (dateFormat == undefined || dateFormat == '') {
                //default ==> should be client specific
                return showTime ? "yyyy-MM-dd hh:ii" : "yyyy-MM-dd";
            } else {
                dateFormat = dateFormat.replace('mm', 'ii');
                dateFormat = dateFormat.replace('MM', 'mm');
                dateFormat = dateFormat.replace('HH', 'hh');
                if (!showTime) {
                    //the format and the showtime flag are somehow conflitant, let´s adjust the format
                    dateFormat = dateFormat.replace('hh:ii', '');
                }
                return dateFormat.trim();
            }
        }


    };

});