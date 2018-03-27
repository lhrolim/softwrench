(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .service('crudextraService', ["$http", "$rootScope", "printService", "alertService", "submitServiceCommons", "redirectService", function ($http, $rootScope, printService, alertService, submitServiceCommons, redirectService) {
            return {
                deletefn: function (schema, datamap, extraParameters, skipAlert) {
                    const fields = datamap;
                    const idFieldName = schema.idFieldName;
                    const userIdFieldName = schema.userIdFieldName;
                    const applicationName = schema.applicationName;
                    const id = fields[idFieldName];
                    const userId = fields[userIdFieldName];

                    const request = () => {
                        var parameters = submitServiceCommons.createSubmissionParameters(fields, schema, null, id);
                        parameters.siteId = fields["siteid"];
                        parameters.orgId = fields["orgid"];
                        if (extraParameters) {
                            parameters = $.extend({}, parameters, extraParameters);
                        }
                        const deleteParams = $.param(parameters);
                        const deleteUrl = removeEncoding(url(`/api/data/${applicationName}/?${deleteParams}`));
                        $http.delete(deleteUrl).then(() => {
                            //TODO: improve this solution, since not every app would have a list schema
                            redirectService.goToApplication(applicationName, "list");
                        });
                    }

                    if (skipAlert) {
                        request();
                    } else {
                        alertService.confirm(null, applicationName, userId).then(request);
                    }
                },

                printDetail: function (schema, datamap) {
                    printService.printDetail(schema, datamap[schema.idFieldName]);
                }
            };

        }]);

})(angular);