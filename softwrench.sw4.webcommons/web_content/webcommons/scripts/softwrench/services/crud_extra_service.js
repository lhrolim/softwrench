(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .factory('crudextraService', ["$http", "$rootScope", "printService", "alertService", "submitService", "redirectService", function ($http, $rootScope, printService, alertService, submitService, redirectService) {
            return {
                deletefn: function (schema, datamap, extraParameters, skipAlert) {
                    const fields = datamap;
                    const idFieldName = schema.idFieldName;
                    const applicationName = schema.applicationName;
                    const id = fields[idFieldName];

                    const request = () => {
                        var parameters = submitService.createSubmissionParameters(fields, schema, null, id);
                        parameters.siteId = fields["siteid"];
                        if (extraParameters) {
                            parameters = $.extend({}, parameters, extraParameters);
                        }
                        const deleteParams = $.param(parameters);
                        const deleteUrl = removeEncoding(url(`/api/data/${applicationName}/?${deleteParams}`));
                        $http.delete(deleteUrl).success(() => {
                            //TODO: improve this solution, since not every app would have a list schema
                            redirectService.goToApplication(applicationName, "list");
                        });
                    }

                    if (skipAlert) {
                        request();
                    } else {
                        alertService.confirm(null, applicationName, id).then(request);
                    }
                },

                printDetail: function (schema, datamap) {
                    printService.printDetail(schema, datamap[schema.idFieldName]);
                }
            };

        }]);

})(angular);