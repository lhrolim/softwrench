var app = angular.module('sw_layout');

app.directive('messagesection', function (contextService) {
    return {
        restrict: 'E',
        //TODO: missing scope isolation here
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

                //console.log(data.successMessage, data);

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

            $scope.showErrorList = function () {
                var isLoggedIn = sessionStorage['ctx_loggedin'];
                if (isLoggedIn !== "true") {
                    //this is a workaround for the COMSW-40. Couldn´t track down why this was needed at all
                    return false;
                }
                return $rootScope.hasErrorList;
            }

            $scope.showErrorDetail = function () {
                var isLoggedIn = sessionStorage['ctx_loggedin'];
                if (isLoggedIn !== "true") {
                    //this is a workaround for the COMSW-40. Couldn´t track down why this was needed at all
                    return false;
                }
                return $rootScope.hasErrorDetail;
            }

            function allowSuccessMessageDisplay(data) {
                log.debug('allowSuccessMessageDisplay#enter');

                var allow = true;
                if (!nullOrUndef(data.schema)) {
                    if (data.schema.schemaId && data.schema.schemaId.indexOf("Summary") > -1) {
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

            function buildFullError(errordata) {
                if (!errordata) return null;
                var message = errordata.errorMessage;
                var type = errordata.errorType;
                var outline = errordata.outlineInformation;
                var fullstack = errordata.fullStack || errordata.errorStack;
                return {
                    message: message,
                    type: type,
                    outline: outline,
                    fullstack: fullstack,
                    text: ("Error description:\n\n" +
                            "Type: {0}\n\n" +
                            "Message: {1}\n\n" +
                            "Outline:\n{2}\n\n" +
                            "StackTrace:\n{3}\n\n")
                            .format(type, message, outline, fullstack)
                }
            }

            $scope.$on('sw_ajaxerror', function (event, errordata) {

                if ($rootScope.showingModal && !event.currentScope.modalshown) {
                    return;
                }

                log.debug('sw_ajaxerror#enter');
                var innerException;
                var limit = 3; // to avoid unwanted infinite recursion
                var i = 0;
                var prependMessage = errordata.prependMessage;

                while (errordata.hasOwnProperty("innerException") && i < limit) {
                    innerException = errordata.innerException;
                    errordata = errordata.innerException;
                    i++;
                }
                if (innerException != null) {
                    errordata = {};
                    errordata.errorStack = innerException.stackTrace;
                    errordata.errorMessage = innerException.message;
                }
                if (prependMessage) {
                    errordata.errorMessage = prependMessage + " --> " + errordata.errorMessage;
                }

                $scope.errorMsg = errordata.errorMessage;
                $scope.errorStack = errordata.errorStack;
                if (!$scope.errorMsg) {
                    //this will happen if the exception is thrown before the Controllers are called, by the .net framework itself; the GenericExceptionFilter would never get called
                    $scope.errorMsg = errordata.exceptionMessage;
                }
                if (!$scope.errorStack) {
                    //this will happen if the exception is thrown before the Controllers are called, by the .net framework itself; the GenericExceptionFilter would never get called
                    $scope.errorStack = errordata.stackTrace;
                }

                $scope.fullError = buildFullError(errordata);
                
                $scope.$broadcast('sw_errormessage', true);

//                if (nullOrUndef($rootScope.hasErrorDetail) && nullOrUndef($rootScope.hasErrorList)) {
                    $rootScope.hasErrorDetail = true;
//                }
            });

            $scope.$on('sw_validationerrors', function (event, validationArray,modal) {
                log.debug('sw_validationerrors#enter');
                if (modal && !event.currentScope.modalshown) {
                    return;
                }

                if (validationArray == null || validationArray.length <= 0) {
                    return;
                }

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
                $scope.fullError = null;
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



