
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

        $scope.redirectURL = function () {
            var redirectUrl = getUrlParameter("ReturnUrl");
            var hash = window.location.hash;
            if (hash && hash.startsWith("##state=")) {
                redirectUrl += hash;
            }
            return redirectUrl;
        }
    }
})(angular);

