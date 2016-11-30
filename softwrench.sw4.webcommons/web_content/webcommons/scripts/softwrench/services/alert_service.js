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
        const confirm = function (msg, applicationName, applicationId) {
            var deferred = $q.defer();
            const defaultConfirmMsg = !applicationName ? "Are you sure you want to delete this?" : "Are you sure you want to delete {0} {1}?".format(applicationName, applicationId);
            const alertMessage = !msg ? i18NService.get18nValue('general.defaultcommands.delete.confirmmsg', defaultConfirmMsg, [applicationName, applicationId]) : msg;

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
        };

        /**
        * @param {string} msg
        * @param {string} applicationName
        * @param {string} applicationId
        */
        const confirmCancel = function (msg, applicationName, applicationId) {
            var deferred = $q.defer();
            const defaultConfirmMsg = !applicationName ? "Are you sure you want to cancel this?" : "Are you sure you want to cancel {0} {1}?".format(applicationName, applicationId);
            const alertMessage = !msg ? i18NService.get18nValue('general.defaultcommands.delete.confirmmsg', defaultConfirmMsg, [applicationName, applicationId]) : msg;

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
        };

        /**
        * @param {string} msg
        */
        const alert = function (msg) {
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
        };

        const notifymessage = function (type, body, title, exceptionType, exceptionOutline, exceptionStack) {
            notificationViewModel.createNotification(type, null, body, exceptionType, exceptionOutline, exceptionStack);
        };

        const success = (body) => {
            notificationViewModel.createNotification("success", null, body);
        };

        const buildException = function (data) {
            //process the innerException, if present
            var innerException;
            const limit = 3; // to avoid unwanted infinite recursion
            var i = 0;
            const prependMessage = data.prependMessage;

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
            const message = data.errorMessage || data.exceptionMessage;
            const exception = {};
            exception.type = data.errorType || data.exceptionType;
            exception.outline = data.outlineInformation;
            exception.stack = data.errorStack || (data.fullStack || data.stackTrace);
            exception.message = message;
            return exception;
        };

        /// <summary>
        /// Use this method to convert an exception to an error notification with more info
        /// </summary>
        /// <param name="data" type="object">The exception infomation</param>
        /// <returns></returns>
        const notifyexception = function (data) {
            if (typeof (data) == 'undefined') {
                return;
            }
            //get the message data
            const exception = this.buildException(data);
            notificationViewModel.createNotification('error', null, exception.message, exception.type, exception.outline, exception.stack);
        };

        const notifyWarning = function (data) {
            if (typeof (data) == 'undefined' || !data.warningDto) {
                return;
            }
            //get the message data
            let message = "";
            //get the message data
            message += data.warningDto.warnMessage;
            const exception = this.buildException(data.warningDto);
            if (data.successMessage) {
                this.notifymessage('success', data.successMessage);
            }


            notificationViewModel.createNotification('warning', null, message, exception.type, exception.message, exception.stack);
        };

        const service = {
            confirm,
            confirmCancel,
            buildException,
            alert,
            notifymessage,
            notifyexception,
            notifyWarning,
            success
        };

        //display notification on JS error
        $(window).on('error', function (evt) {
            notificationViewModel.processJsError(evt);
        });

        return service;
    }
})(angular);
