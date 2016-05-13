
(function (angular) {
    'use strict';

    function userStatisticsController($scope, restService, alertService, formatService) {

        var locatePrimaryEmail = function (compositionEmails) {
            if (!compositionEmails) {
                return null;
            }
            for (var i = 0; i < compositionEmails.length; i++) {
                var email = compositionEmails[i];
                if (email.isprimary) {
                    return email.emailaddress;
                }
            }
            return null;
        };

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
            var userid = $scope.datamap["#userid"];
            var email = locatePrimaryEmail($scope.datamap["email_"]);

            if (email == null) {
                alertService.alert("This user has no email registered. Please setup a primary email first");
                return;
            }

            var params = {
                userId: userid,
                email: email
            }

            restService.postPromise("UserSetupWebApi", "SendActivationEmail", params)
                .then(function() {
                    alertService.notifymessage("success", "An email has been sent with instructions to reset the password", "Email Sent");
                });
        }
    }

    angular.module("sw_layout")
        .controller("UserStatisticsController", ["$scope", "restService", "alertService", "formatService", userStatisticsController]);

})(angular);

