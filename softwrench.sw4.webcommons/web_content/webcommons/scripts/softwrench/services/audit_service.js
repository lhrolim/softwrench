var app = angular.module('sw_layout');

app.factory('auditService', function ($rootScope, restService) {

    return {
        insertAuditEntry: function (application, id, action, datamap) {
            var jsonData = angular.toJson(datamap);
            var httpParameters = {
                application: application,
                id: id,
                action: action
            };
            restService.invokePost("audit", "post", httpParameters, jsonData, function () {
                return;
            }, function () {
                return;
            });
        }
    };

});


