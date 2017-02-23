var app = angular.module('sw_layout');

app.directive('messagesection', function (contextService, $timeout, logoutService) {
    "ngInject";
    return {
        restrict: 'E',
        templateUrl: contextService.getResourceUrl('Content/Templates/message_section.html'),
        controller: function ($scope, i18NService, $rootScope, fixHeaderService) {

            $scope.contextPath = function (path) {
                return url(path);
            };

            $scope.$on('sw_checksuccessmessage', function (event, data) {
                if (!nullOrUndef(data.schema)) {
                    var schema = data.schema;
                    if (schema == 'list' && $rootScope.showSuccessMessage && $scope.successMsg != null) {
                        $scope.hasSuccessList = true;
                        $scope.hasSuccessDetail = false;
                    }
                }
            });

            $scope.$on('sw_successmessage', function (event, data) {
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

                } else {
                    hideSuccessMessage();
                }
            });

            function allowSuccessMessageDisplay(data) {
                var allow = true;
                if (!nullOrUndef(data.schema)) {
                    if (data.schema.schemaId.indexOf("Summary") > -1) {
                        allow = false;
                    }
                }
                return allow;
            }

            function hideSuccessMessage() {
                $scope.hasSuccessList = false;
                $scope.hasSuccessDetail = false;
                $scope.successMsg = null;
                $rootScope.showSuccessMessage = false;
            }

            $scope.$on('sw_successmessagetimeout', function (event, data) {
                hideSuccessMessage();
            });

            $scope.$on('sw_ajaxerror', function (event, errordata, status) {
                if (status == 403) {
                    logoutService.logout();
                    return;
                }

                if ($scope.errorMsg) {
                    return;
                }

                $scope.errorMsg = errordata.errorMessage;
                $scope.errorStack = errordata.errorStack;
                $scope.$broadcast('sw_errormessage', true);

                if (nullOrUndef($rootScope.hasErrorDetail) && nullOrUndef($rootScope.hasErrorList)) {
                    $rootScope.hasErrorDetail = true;
                }
            });

            $scope.$on('sw_validationerrors', function (event, validationArray) {
                $scope.hasValidationError = true;
                $scope.validationArray = validationArray;
                $('html, body').animate({ scrollTop: 0 }, 'fast');
            });

            $scope.$on('sw_ajaxinit', function (event, errordata) {
                $scope.removeAlert();
                $scope.removeValidationAlert();
            });

            $scope.$on('sw_cancelclicked', function (event, errordata) {
                $scope.removeAlert();
                $scope.removeValidationAlert();
            });

            $scope.removeAlert = function () {
                $rootScope.hasErrorDetail = false;
                $rootScope.hasErrorList = false;
                $scope.errorMsg = null;
                $scope.errorStack = null;
                $scope.$broadcast('sw_errormessage', false);
            };

            $scope.removeValidationAlert = function () {
                $scope.hasValidationError = false;
                $scope.validationArray = null;
            };

            $scope.openModal = function () {
                $('#errorModal').modal('show');
                $("#errorModal").draggable();
            };

            $scope.hideModal = function () {
                $('#errorModal').modal('hide');
            };

            $scope.i18N = function (key, defaultValue, paramArray) {
                return i18NService.get18nValue(key, defaultValue, paramArray);
            };

            $scope.closeSuccessMessage = function() {
                hideSuccessMessage();
            }
        }
    };
});



