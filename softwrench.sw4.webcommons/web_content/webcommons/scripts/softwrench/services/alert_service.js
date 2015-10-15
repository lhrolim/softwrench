var app = angular.module('sw_layout');

app.factory('alertService', function ($rootScope, $timeout, i18NService, $log) {
    var log = $log.getInstance('sw4.alertService');

    /// <summary>
    /// This internal method will process the input and create the user notification message.
    /// </summary>
    function createNotification(type, title, body, exceptionType, exceptionOutline, exceptionStack) {

        //build the message object
        var message = {};
        message.type = type;
        message.title = title;
        message.body = body;

        //if any exception info is present, create the exception object
        if (exceptionType || exceptionOutline || exceptionStack) {
            var exception = {};
            exception.type = exceptionType;
            exception.outline = exceptionOutline;
            exception.stack = exceptionStack;
            message.exception = exception;
        }

        //broad the notification event
        if (message.body) {

            //TODO: is this the best way to call the notifications?
            $rootScope.$broadcast('sw_notificationmessage', message);
        }
    }

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

        /// <summary>
        /// Use this method to display user notifications.
        /// </summary>
        /// <param name="body">The user message</param>
        /// <param name="title">Optional, Override the default notification title</param>
        /// <param name="exceptionType">Optional, if present, will display the more info button/link</param>
        /// <param name="exceptionOutline">Optional, if present, will display the more info button/link</param>
        /// <param name="exceptionStack">Optional, if present, will display the more info button/link</param>
        notifymessage: function (type, body, title, exceptionType, exceptionOutline, exceptionStack) {
            createNotification(type, null, body, exceptionType, exceptionOutline, exceptionStack);
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

                createNotification('error', null, message, exception.type, exception.outline, exception.stack);
            }
        }
    };
});


