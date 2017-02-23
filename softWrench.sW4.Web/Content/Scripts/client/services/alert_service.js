var app = angular.module('sw_layout');

app.factory('alertService', function ($rootScope, $timeout, i18NService) {
    "ngInject";
    return {

        confirm: function (applicationName, applicationId, callbackFunction, msg, cancelcallback) {
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
        },
        confirmCancel: function (scope, prevdata, prevschema, applicationName, applicationId, callbackFunction, msg, cancelcallback) {
            var defaultConfirmMsg = "Are you sure you want to cancel {0} {1}?".format(applicationName, applicationId);
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
                    
                        scope.cancelfn({ data: scope.previousdata, schema: scope.previousschema });
                        scope.$emit('sw_cancelclicked');
                        return;
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

        success: function (message, autoHide) {
            var data = { successMessage: message };
            $rootScope.$broadcast('sw_successmessage', data);
            if (autoHide) {
                $timeout(function () {
                    data.successMessage = null;
                    $rootScope.$broadcast('sw_successmessage', data);
                }, 5000);
            }
        },

        error: function (message, autoHide) {
            var data = { errorMessage: message };
            $rootScope.$broadcast('sw_errormessage', data);
            if (autoHide) {
                $timeout(function () {
                    data.errorMessage = null;
                    $rootScope.$broadcast('sw_errormessage', data);
                }, 5000);
            }
        }
    };
});


