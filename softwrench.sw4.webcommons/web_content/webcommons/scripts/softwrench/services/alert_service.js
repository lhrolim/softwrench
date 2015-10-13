var app = angular.module('sw_layout');

app.factory('alertService', function ($rootScope, $timeout, i18NService) {

    return {

        confirmMsg: function (msg, callbackFunction, cancelcallback) {
            this.confirm(null, null, callbackFunction, msg, cancelcallback);
        },

        confirm: function (applicationName, applicationId, callbackFunction, msg, cancelcallback) {
            //TODO: refactor to use promises
            var defaultConfirmMsg = "Are you sure you want to delete {0} {1}?".format(applicationName, applicationId);
            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            var defaultDeleteMsg = i18NService.get18nValue('general.defaultcommands.delete.confirmmsg', defaultConfirmMsg, [applicationName, applicationId]);
            bootbox.confirm({
                message: msg == null ? defaultDeleteMsg : msg,
                title: i18NService.get18nValue('general.defaultcommands._confirmationtitle', 'Confirmation'),
                className: 'smallmodal',
                callback: function (result) {
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
        confirmCancel: function (applicationName, applicationId, callbackFunction, msg, cancelcallback) {
            var defaultConfirmMsg = "Are you sure you want to cancel {0} {1}?".format(applicationName, applicationId);
            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            var defaultDeleteMsg = i18NService.get18nValue('general.defaultcommands.delete.confirmmsg', defaultConfirmMsg, [applicationName, applicationId]);
            bootbox.cancelDialog({
                message: msg == null ? defaultDeleteMsg : msg,
                title: i18NService.get18nValue('general.defaultcommands._confirmationtitle', 'Confirmation'),
                className: 'smallmodal',
                callback: function (result) {

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

        alert: function (msg) {
            bootbox.setDefaults({ locale: i18NService.getCurrentLanguage() });
            bootbox.alert({
                message: msg,
                title: i18NService.get18nValue('general.defaultcommands._alert', 'Alert'),
                className: 'smallmodal',
            });
        },

        /// <summary>
        /// Use this method to display the green success message on the top of the system.
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="autoHide">whether the message should hide after some time</param>
        /// <param name="timeout">the time that the messa will be displayed on the screen. Only makes sense if autoHide is true. Defaults to 5000</param>/
        success: function (message_body, autoHide, timeout) {
            var message = {};
            message.type = 'success';
            message.body = message_body;
            $rootScope.$broadcast('sw_notificationmessage', message);
        },

        info: function (message_body) {
            var message = {};
            message.type = 'info';
            message.body = message_body;
            $rootScope.$broadcast('sw_notificationmessage', message);
        },

        error: function (message_body) {
            var message = {};
            message.type = 'error';
            message.body = message_body;
            $rootScope.$broadcast('sw_notificationmessage', message);
        }

    };

});


