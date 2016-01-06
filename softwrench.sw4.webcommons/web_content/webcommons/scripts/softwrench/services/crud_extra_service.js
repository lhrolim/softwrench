(function (angular) {
    "use strict";

angular.module('sw_layout')
    .factory('crudextraService', ["$http", "$rootScope", "printService", "alertService", "submitService", function ($http, $rootScope, printService, alertService, submitService) {

    return {
        deletefn: function (schema, datamap) {
            var idFieldName = schema.idFieldName;
            var applicationName = schema.applicationName;
            var id = datamap[idFieldName];
            alertService.confirm(applicationName, id, function () {
                var parameters = submitService.createSubmissionParameters();
                var deleteParams = $.param(parameters);
                var deleteURL = removeEncoding(url("/api/data/" + applicationName + "/" + id + "?" + deleteParams));
                $http.delete(deleteURL)
                    .success(function (data) {
                        $rootScope.$broadcast('sw_applicationrenderviewwithdata', data);
                    });
            });
        },

        printDetail: function (schema, datamap) {
            printService.printDetail(schema, datamap[schema.idFieldName]);
        }
    };

}]);

})(angular);