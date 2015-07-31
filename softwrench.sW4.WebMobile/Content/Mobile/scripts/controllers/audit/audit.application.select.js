(function (app) {
    "use strict";

    app.controller("AuditApplicationSelectController",
        ["$scope", "offlineAuditService", "securityService", "routeService",
            function ($scope, offlineAuditService, securityService, routeService) {

                var currentUser = securityService.currentUser();

                $scope.data = {
                    applications: []
                };

                var loadApplications = function () {
                    offlineAuditService.listAudittedApplications(currentUser)
                        .then(function (applications) {
                            $scope.data.applications = applications;
                        });
                };

                $scope.selectApplication = function (application) {
                    routeService.go(".entrylist", { application: application });
                };

                loadApplications();

            }]);

})(softwrench);
