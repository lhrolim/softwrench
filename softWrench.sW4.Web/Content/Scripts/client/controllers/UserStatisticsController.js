
(function (angular) {
    'use strict';

    function userStatisticsController($scope, restService, alertService, formatService, userService) {

      

        $scope.activationlink = $scope.datamap["activationlink"];

        $scope.getFormattedDate = function (date) {
            return formatService.formatDate(date);
        }


        $scope.hasAlreadyLoggedIn = function () {
            return $scope.datamap["statistics"] != null;
        }

        $scope.isLdap = function () {
            return "true" === $scope.datamap["ldapEnabled"];
        }

        $scope.sendActivationEmail = function () {
            const userid = $scope.datamap["#userid"];
            const email = userService.locatePrimaryEmail($scope.datamap["email_"]);
            if (email == null) {
                alertService.alert("This user has no email registered. Please setup a primary email first");
                return;
            }
            const params = {
                userId: userid,
                email: email
            };
            restService.postPromise("UserSetupWebApi", "SendActivationEmail", params)
                .then(function() {
                    alertService.notifymessage("success", "An email has been sent with instructions to reset the password", "Email Sent");
                });
        }
    }

    angular.module("sw_layout")
        .controller("UserStatisticsController", ["$scope", "restService", "alertService", "formatService", "userService", userStatisticsController]);

})(angular);

