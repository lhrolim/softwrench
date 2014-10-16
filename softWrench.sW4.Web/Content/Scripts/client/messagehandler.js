var app = angular.module('sw_layout');

app.directive('messagesection', function (contextService) {
    return {
        restrict: 'E',
        templateUrl: contextService.getResourceUrl('Content/Templates/message_section.html'),
        controller: function ($scope, i18NService, $rootScope, fixHeaderService, $log, $timeout) {

            var log = $log.getInstance('sw4.messagesection');

            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.$on('sw_checksuccessmessage', function (event, data) {
                log.debug('sw_checksuccessmessage#enter');

                if (!nullOrUndef(data.schema)) {
                    var schema = data.schema;
                    if (schema == 'list' && $rootScope.showSuccessMessage && $scope.successMsg != null) {
                        $scope.hasSuccessList = true;
                        $scope.hasSuccessDetail = false;
                    }
                }
            });

            $scope.$on('sw_successmessage', function (event, data) {
                log.debug('sw_successmessage#enter');

                if (!nullOrUndef(data.successMessage) && allowSuccessMessageDisplay(data)) {
                    if (nullOrUndef(data.schema)) {
                        $scope.hasSuccessDetail = true;
                    } else {
                        if (data.schema.stereotype == "List") {
                            $scope.hasSuccessList = true;
                        } else {
                            $scope.hasSuccessDetail = true;
                        }
                    }
                    $scope.successMsg = data.successMessage;
                    $rootScope.showSuccessMessage = true;
                    fixHeaderService.fixSuccessMessageTop(true);
                } else {
                    hideSuccessMessage();
                }

                fixHeaderService.callWindowResize();
            });

            function allowSuccessMessageDisplay(data) {
                log.debug('allowSuccessMessageDisplay#enter');

                var allow = true;
                if (!nullOrUndef(data.schema)) {
                    if (data.schema.schemaId.indexOf("Summary") > -1) {
                        allow = false;
                    }
                }
                return allow;
            }

            function hideSuccessMessage() {
                log.debug('hideSuccessMessage#enter');

                $scope.hasSuccessList = false;
                $scope.hasSuccessDetail = false;
                $scope.successMsg = null;
                $rootScope.showSuccessMessage = false;

                fixHeaderService.callWindowResize();
            }

            $scope.$on('sw_successmessagetimeout', function (event, data) {
                log.debug('sw_successmessagetimeout#enter');

                hideSuccessMessage();
            });

            $scope.$on('sw_ajaxerror', function (event, errordata) {
                log.debug('sw_ajaxerror#enter');

                $scope.errorMsg = errordata.errorMessage;
                $scope.errorStack = errordata.errorStack;
                $scope.$broadcast('sw_errormessage', true);

                if (nullOrUndef($rootScope.hasErrorDetail) && nullOrUndef($rootScope.hasErrorList)) {
                    $rootScope.hasErrorDetail = true;
                }
            });

            $scope.$on('sw_validationerrors', function (event, validationArray) {
                log.debug('sw_validationerrors#enter');

                $scope.hasValidationError = true;
                $scope.validationArray = validationArray;
                $('html, body').animate({ scrollTop: 0 }, 'fast');

                fixHeaderService.callWindowResize();
            });

            $scope.$on('sw_ajaxinit', function (event, errordata) {
                $scope.removeAlert();
                $scope.removeValidationAlert();
            });

            $scope.$on('sw_cancelclicked', function (event, errordata) {
                log.debug('sw_cancelclicked#enter');

                $scope.removeAlert();
                $scope.removeValidationAlert();
            });

            $scope.removeAlert = function () {
                log.debug('removeAlert#enter');

                $rootScope.hasErrorDetail = false;
                $rootScope.hasErrorList = false;
                $scope.errorMsg = null;
                $scope.errorStack = null;
                $scope.$broadcast('sw_errormessage', false);

                fixHeaderService.callWindowResize();
            };

            $scope.removeValidationAlert = function () {
                log.debug('removeValidationAlert#enter');

                $scope.hasValidationError = false;
                $scope.validationArray = null;

                fixHeaderService.callWindowResize();
            };

            $scope.openModal = function () {
                $('#errorModal').modal('show');
                $("#errorModal").draggable();

                fixHeaderService.callWindowResize();
            };

            $scope.hideModal = function () {
                $('#errorModal').modal('hide');

                fixHeaderService.callWindowResize();
            };

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };
        }
    };
});



