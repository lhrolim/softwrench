(function (angular) {
    "use strict";

    angular.module('webcommons_services')
        .factory('alertService', ["$rootScope", "$timeout", "i18NService", "notificationViewModel", "$log", "$q", alertService]);

    function alertService($rootScope, $timeout, i18NService, notificationViewModel, $log, $q) {
        /**
        * @param {string} msg
        * @param {string} applicationName
        * @param {string} applicationId
        */
        var confirm = function (msg, applicationName, applicationId) {
            var deferred = $q.defer();
            var defaultConfirmMsg = !applicationName ? "Are you sure you want to delete this?": "Are you sure you want to delete {0} {1}?".format(applicationName, applicationId);
            var alertMessage = !msg ? i18NService.get18nValue('general.defaultcommands.delete.confirmmsg', defaultConfirmMsg, [applicationName, applicationId]) : msg;

            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            bootbox.confirm({
                templates: {
                    header:
                        "<div class='modal-header'>" +
                        "<i class='fa fa-question-circle'></i>" +
                        "<h4 class='modal-title'></h4>" +
                        "</div>"
                },
                message: alertMessage,
                title: i18NService.get18nValue('general.defaultcommands._confirmationtitle', 'Confirm...'),
                className: 'smallmodal',
                callback: function (result) {
                    result ? deferred.resolve() : deferred.reject();
                }
            });

            return deferred.promise;
        }

        /**
        * @param {string} msg
        * @param {string} applicationName
        * @param {string} applicationId
        */
        var confirmCancel = function (msg, applicationName, applicationId) {
            var deferred = $q.defer();
            var defaultConfirmMsg = !applicationName ? "Are you sure you want to cancel this?" : "Are you sure you want to cancel {0} {1}?".format(applicationName, applicationId);
            var alertMessage = !msg ? i18NService.get18nValue('general.defaultcommands.delete.confirmmsg', defaultConfirmMsg, [applicationName, applicationId]) : msg
           
            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            bootbox.cancelDialog({
                templates: {
                    header:
                        "<div class='modal-header'>" +
                        "<i class='fa fa-question-circle'></i>" +
                        "<h4 class='modal-title'></h4>" +
                        "</div>"
                },
                message: alertMessage,
                title: i18NService.get18nValue('general.defaultcommands._confirmationtitle', 'Confirm...'),
                className: 'smallmodal',
                callback: function (result) {
                    result ? deferred.resolve() : deferred.reject();
                }
            });

            return deferred.promise;
        }

        /**
        * @param {string} msg
        */
        var alert = function (msg) {
            var deferred = $q.defer();

            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            bootbox.alert({
                templates: {
                    header:
                        "<div class='modal-header'>" +
                        "<i class='fa fa-times-circle'></i>" +
                        "<h4 class='modal-title'></h4>" +
                        "</div>"
                },
                message: msg,
                title: i18NService.get18nValue('general.defaultcommands._alert', 'Sorry...'),
                className: 'smallmodal error',
                callback: function () {
                    deferred.resolve();
                }
            });
            return deferred.promise;
        }

        var notifymessage = function (type, body, title, exceptionType, exceptionOutline, exceptionStack) {
            notificationViewModel.createNotification(type, null, body, exceptionType, exceptionOutline, exceptionStack);
        }

        /// <summary>
        /// Use this method to convert an exception to an error notification with more info
        /// </summary>
        /// <param name="data" type="object">The exception infomation</param>
        /// <returns></returns>
        var notifyexception = function (data) {
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

        const service = {
            confirm,
            confirmCancel,
            alert,
            notifymessage,
            notifyexception
        };

        return service;
    }
})(angular);