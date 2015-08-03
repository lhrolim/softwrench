(function (app) {
    "use strict";

    app.controller("AuditApplicationSelectController",
        ["$scope", "offlineAuditService", "routeService",
            function ($scope, offlineAuditService, securityService, routeService) {

                $scope.data = {
                    applications: []
                };

                var loadApplications = function () {
                    offlineAuditService.listAudittedApplications()
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
