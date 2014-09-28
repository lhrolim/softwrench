var app = angular.module('sw_layout');

app.factory('crudextraService', function ($http, $rootScope, printService, alertService) {

    return {

        deletefn: function (schema, datamap) {
            var idFieldName = schema.idFieldName;
            var applicationName = schema.applicationName;
            var id = datamap[idFieldName];
            alertService.confirm(applicationName, id, function () {
                var parameters = {};
                if (sessionStorage.mockmaximo == "true") {
                    parameters.mockmaximo = true;
                }
                parameters.platform = platform();
                parameters = addSchemaDataToParameters(parameters, schema);
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

});


