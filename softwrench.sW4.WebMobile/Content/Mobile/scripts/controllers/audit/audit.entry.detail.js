(function (app) {
    "use strict";

    app.controller("AuditEntryDetailController",
        ["$scope", "offlineAuditService", "formatService", "$stateParams", "$ionicHistory",
            function ($scope, offlineAuditService, formatService, $stateParams, $ionicHistory) {

                $scope.data = {
                    entry: {},
                    entity: {}
                };

                $scope.back = function() {
                    $ionicHistory.goBack();
                };

                var formatEntry = function(entry) {
                    entry.formattedDate = formatService.formatDate(entry.createDate, "MM/dd/yyyy HH:mm");
                    return entry;
                };

                var loadData = function (id) {
                    offlineAuditService.getAuditEntry(id)
                        .then(function (entry) {
                            $scope.data.entry = formatEntry(entry);

                            return offlineAuditService.getTrackedEntity(entry);
                        })
                        .then(function(entity) {
                            $scope.data.entity = entity;
                        });
                };

                loadData($stateParams.id);

            }]);

})(softwrench);
