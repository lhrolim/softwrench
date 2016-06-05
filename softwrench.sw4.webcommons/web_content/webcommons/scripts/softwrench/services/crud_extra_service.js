(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('crudextraService', ["$http", "$rootScope", "printService", "alertService", "submitService", "redirectService", function ($http, $rootScope, printService, alertService, submitService, redirectService) {

    return {
        deletefn: function (schema, datamap) {
            var fields = datamap.fields;
            var idFieldName = schema.idFieldName;
            var applicationName = schema.applicationName;
            var id = fields[idFieldName];
            var userId = fields[schema.userIdFieldName];

            alertService.confirm(null, applicationName, id).then(function () {
                var parameters = submitService.createSubmissionParameters(fields, schema, null, id);
                parameters.userId = userId;
                parameters.siteId = fields["siteid"];
                var deleteParams = $.param(parameters);
                var deleteURL = removeEncoding(url("/api/data/" + applicationName + "/?" + deleteParams));
                $http.delete(deleteURL)
                    .success(function (data) {
                        //TODO: improve this solution, since not every app would have a list schema
                        redirectService.goToApplication(applicationName, "list");
                    });
            });

        },

       


        printDetail: function (schema, datamap) {
            printService.printDetail(schema, datamap[schema.idFieldName]);
        }
    };

}]);

})(angular);