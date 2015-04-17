var app = angular.module('sw_layout');

app.factory('expressionService', function ($rootScope, restService) {

    return {
        insertAuditEntry: function (application, id, action, jsonData) {
            var jsonString = angular.toJson(jsonData);
            var httpParameters = {
                application: application,
                id: id,
                action: action
            };
            restService.invokePost("audit", "post", httpParameters, jsonString, function () {
                return;
            }, function () {
                return;
            });
        }
    };

});


