
(function (angular) {
    'use strict';

    angular.module('sw_prelogin').controller('SignInController', ['$scope', "$http", SignInController]);

    function SignInController($scope, $http) {


        $scope.showforgotPasswordForm = false;

        $scope.forgotPassword = function () {
            var postUrl = url("/api/generic/UserSetupWebApi/ForgotPassword?userNameOrEmail={0}".format($scope.userNameOrEmail));
            $http.post(postUrl).success(function(data) {
                $scope.showforgotPasswordForm = false;
                $scope.errorMessage = null;
                $scope.successMessage = "An email has been sent with instructions to reset the password";
                $scope.userNameOrEmail = null;
            }).error(function (err) {
                $scope.errorMessage = err.errorMessage;
            });
        }

        $scope.redirectUrl = function () {
            if (!window.location.origin) {
                // ie9 workaround
                window.location.origin = window.location.protocol + "//" + window.location.hostname + (window.location.port ? ":" + window.location.port : "");
            }
            return window.location.href.substring(window.location.origin.length);
        }
    }
})(angular);

