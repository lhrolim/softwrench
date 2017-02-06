(function (angular) {
    'use strict';

    angular.module('sw_prelogin').controller('SignInController', ['$scope', "$http", "alertService", SignInController]);

    function SignInController($scope, $http, alertService) {
        $scope.showforgotPasswordForm = false;

        $scope.forgotPassword = function () {
            var postUrl = url("/api/generic/UserSetupWebApi/ForgotPassword?userNameOrEmail={0}".format($scope.userNameOrEmail));
            $http.post(postUrl).then(function () {
                $scope.showforgotPasswordForm = false;
                $scope.errorMessage = null;
                $scope.successMessage = "An email has been sent with instructions to reset the password";
                $scope.userNameOrEmail = null;
            }).catch(function (err) {
                $scope.errorMessage = err.errorMessage;
            });
        };

        $scope.redirectUrl = function () {
            if (!window.location.origin) {
                // ie9 workaround
                window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ":" + window.location.port : "");
            }
            return window.location.href.substring(window.location.origin.length);
        };
        

        $scope.registration = (function () {
            var data = {};
            var loading = {
                spinner: null,
                ongoing: false
            };
            var selector = "#newUserRegistrationModal";

            function showLoading() {
                loading.ongoing = true;
                loading.spinner = new window.Spinner().spin(document.body);
            }
            function hideLoading() {
                if (loading.spinner) loading.spinner.stop();
                loading.ongoing = false;
                loading.spinner = null;
            }
            function closeModal() {
                $(selector).modal("hide");
            }
            
            function alertMessage(type, message) {
                window.bootbox.alert({
                    templates: {
                        header:
                          "<div class='modal-header'>" +
                            "<i class='fa " + (type === "success" ? "fa-check-circle" : "fa-times-circle") + "'></i>" +
                            "<h4 class='modal-title'></h4>" +
                          "</div>"
                    },
                    message: message,
                    title: type.charAt(0).toUpperCase() + type.substring(1),
                    className: "smallmodal " + type
                });
            }

            function buildRequestDto() {
                return {
                    personuid: "-1",
                    firstname: data.firstname,
                    lastname: data.lastname,
                    personid: data.username,
                    '#personid': data.username,
                    '#password': data.password,
                    '#retypepassword': data.passwordconfirm,
                    '#primaryemail': data.email,
                    '#primaryphone': data.phone,
                    '#signature': "",
                    'isactive': false
                };
            }

            function submit() {
                showLoading();
                var action = url("/api/generic/UserSetupWebApi/NewUserRegistration");
                //var action = url("/api/data/person");
                var request = buildRequestDto();
                return $http.post(action, request)
                    .then(function (response) {
                        alertMessage("success", response.data.successMessage);
                        closeModal();
                        clearViewModel();
                    })
                    .catch(function (error) {
                        var message = !!error.data.errorMessage ? error.data.errorMessage : error.data.message;
                        alertMessage("error", message);
                    })
                    .finally(hideLoading);
            }

            function clearViewModel() {
                $scope.registration.data = {};
                $scope.registration.form.$setPristine();
                $scope.registration.form.$setUntouched();
            }

            function dismiss() {
                hideLoading();
                clearViewModel();
            }

            function setUserName() {
                data.username = !data.firstname || !data.lastname
                    ? ""
                    : data.firstname.toLowerCase().charAt(0) + data.lastname.split(" ").join("").toLowerCase();
            }

            return {
                data: data,
                loading: loading,
                submit: submit,
                dismiss: dismiss,
                setUserName: setUserName
            };
        })();

        if (!loginModel || !loginModel.Error) {
            return;
        }
        const error = loginModel.Error;
        if (error.ErrorStack) {
            const errorData = {
                errorMessage: error.ErrorMessage,
                errorStack: error.ErrorStack,
                errorType: error.ErrorType,
                outlineInformation: error.OutlineInformation
            }
            alertService.notifyexception(errorData);
        } else {
            alertService.notifymessage("error", error.ErrorMessage);
        }
    }

})(angular);

