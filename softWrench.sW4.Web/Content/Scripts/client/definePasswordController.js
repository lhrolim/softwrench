(function (angular) {
    'use strict';

    function DefinePasswordController($scope, $http, spinService, passwordValidationService, configurationService, contextService) {

        contextService.set("sw:changepasword", true);

        $scope.timezone = new Date().getTimezoneOffset();

        $scope.changePassword = $("#hddn_chgpassword").val();
        $scope.username = $("#hddn_username").val();
        $scope.passwordErrors = [];

        $scope.passwordLabel = $scope.changePassword ? "New Password" : "Password";
        $scope.confirmPasswordLabel = $scope.changePassword ? "Confirm New Password" : "Confirm Password";

        $scope.submitEnabled = function () {
            if (!$scope.user) {
                return false;
            }
            const password = $scope.user.password;
            const confirmpassword = $scope.user.confirmpassword;
            return password && password === confirmpassword;
        }

        function submit() {
            contextService.deleteFromContext("sw:changepasword");
            const formToSubmit = $("#definepasswordform");
            const action = $scope.changePassword ? url("/UserSetup/DoChangePassword") : url("/UserSetup/DoSetPassword");
            formToSubmit.attr("action", action);
            formToSubmit.submit();
        }

        function doChangePassword() {
            const newPassword = $("#password").val();
            $http({
                method: "POST",
                url: url("/UserSetup/VerifySamePassword"),
                data: { password: newPassword }
            }).then(response => {
                if (response.data.samePassword === "true") {
                    $scope.passwordErrors.push("The new password have to be different from the current one.");
                }
                if ($scope.passwordErrors.length > 0) {
                    spinService.stop();
                    return;
                }
                submit();
            }).catch(() => {
                $scope.passwordErrors.push("Uknown Error.");
                spinService.stop();
            });
        }

        $scope.innerConfirm = function(passwordErrors) {
            $scope.passwordErrors = passwordErrors;

            if ($scope.changePassword) {
                doChangePassword();
                return;
            }
            
            if ($scope.passwordErrors.length > 0) {
                spinService.stop();
                return;
            }
            submit();
        }

        $scope.confirm = function () {
            spinService.start();
            configurationService.updateConfigurations()
                .then(() => passwordValidationService.validatePasswordAsync($scope.user.password, { username: $scope.username }))
                .then(errors => $scope.innerConfirm(errors));
        }

        $scope.cancel = function () {
            spinService.start();
            contextService.deleteFromContext("sw:changepasword");
            window.location = url("/SignOut/SignOut");
        }

        $scope.$watch("user.password", () => $scope.passwordErrors = []);
        $scope.$watch("user.confirmpassword", () => $scope.passwordErrors = []);
    }

    DefinePasswordController.$inject = ['$scope', '$http', 'spinService', 'passwordValidationService', 'configurationService', 'contextService'];

    angular.module('sw_layout').controller('DefinePasswordController', DefinePasswordController);



})(angular);
