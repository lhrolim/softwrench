var app = angular.module('webcommons_services');

app.factory('alertService', function ($rootScope, $timeout, i18NService, notificationViewModel, $log) {
    var log = $log.getInstance('sw4.alertService');

    return {
        confirmMsg: function(msg, callbackFunction, cancelcallback) {
            this.confirm(null, null, callbackFunction, msg, cancelcallback);
        },

        confirm: function(applicationName, applicationId, callbackFunction, msg, cancelcallback) {
            //TODO: refactor to use promises
            var defaultConfirmMsg = "Are you sure you want to delete {0} {1}?".format(applicationName, applicationId);
            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            var defaultDeleteMsg = i18NService.get18nValue('general.defaultcommands.delete.confirmmsg', defaultConfirmMsg, [applicationName, applicationId]);
            bootbox.confirm({
                message: msg == null ? defaultDeleteMsg : msg,
                title: i18NService.get18nValue('general.defaultcommands._confirmationtitle', 'Confirmation'),
                className: 'smallmodal',
                callback: function(result) {
                    if (result == false) {
                        if (cancelcallback != undefined) {
                            cancelcallback();
                            return;
                        }
                        return;
                    }
                    callbackFunction();
                }
            });
            return false;
        },
        confirmCancel: function(applicationName, applicationId, callbackFunction, msg, cancelcallback) {
            var defaultConfirmMsg = "Are you sure you want to cancel {0} {1}?".format(applicationName, applicationId);
            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            var defaultDeleteMsg = i18NService.get18nValue('general.defaultcommands.delete.confirmmsg', defaultConfirmMsg, [applicationName, applicationId]);
            bootbox.cancelDialog({
                message: msg == null ? defaultDeleteMsg : msg,
                title: i18NService.get18nValue('general.defaultcommands._confirmationtitle', 'Confirmation'),
                className: 'smallmodal',
                callback: function(result) {

                    if (result == false) {
                        if (cancelcallback != undefined) {
                            cancelcallback();
                            return;
                        }
                        return;
                    }
                    callbackFunction();

                }

            });
        },

        alert: function(msg) {
            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            bootbox.alert({
                message: msg,
                title: i18NService.get18nValue('general.defaultcommands._alert', 'Alert'),
                className: 'smallmodal',
            });
        },

        notifymessage: function (type, body, title, exceptionType, exceptionOutline, exceptionStack) {
            notificationViewModel.createNotification(type, null, body, exceptionType, exceptionOutline, exceptionStack);
        },

        /// <summary>
        /// Use this method to convert an exception to an error notification with more info
        /// </summary>
        /// <param name="data" type="object">The exception infomation</param>
        /// <returns></returns>
        notifyexception: function (data) {
            if (typeof (data) != 'undefined') {

                //process the innerException, if present
                var innerException;
                var limit = 3; // to avoid unwanted infinite recursion
                var i = 0;
                var prependMessage = data.prependMessage;

                while (data.hasOwnProperty('innerException') && i < limit) {
                    innerException = data.innerException;
                    data = data.innerException;
                    i++;
                }

                if (innerException != null) {
                    data = {};
                    data.errorStack = innerException.stackTrace;
                    data.errorMessage = innerException.message;
                }

                if (prependMessage) {
                    data.errorMessage = prependMessage + ' --> ' + data.errorMessage;
                }

                //get the message data
                var message = data.errorMessage || data.exceptionMessage;
                var exception = {};
                exception.type = data.errorType || data.exceptionType;
                exception.outline = data.outlineInformation;
                exception.stack = data.errorStack || (data.fullStack || data.stackTrace);

                notificationViewModel.createNotification('error', null, message, exception.type, exception.outline, exception.stack);
            }
        }
    };
});


