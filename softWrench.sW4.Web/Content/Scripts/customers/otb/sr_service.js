(function (angular) {
    "use strict";

    angular.module('sw_layout')
        .service('srService', ["alertService", "redirectService", function (alertService, redirectService) {

            return {


                startRelatedSRCreation: function (schema, datamap) {
                    var localDatamap = datamap;
                    var relatedDatamap = {
                        'status': "NEW",
                        '#relatedrecord_recordkey': localDatamap[schema.userIdFieldName]
                    };
                    const clonedFields = [
                        "affectedemail", "affectedperson", "affectedphone", "assetnum", "assetditeid",
                        "cinum", "classstructureid", "commodity", "commoditygroup",
                        "description", "longdescription_.ldtext", "ld_.ldtext",
                        "glaccount",
                        "location",
                        "orgid",
                        "reportedby", "reportedemail", "reportedphone", "reportedpriority",
                        "siteid", "source",
                        "virtualenv"
                    ];
                    clonedFields.forEach(function (field) {
                        if (!localDatamap.hasOwnProperty(field)) return;
                        relatedDatamap[field] = localDatamap[field];
                    });
                    const params = {
                        postProcessFn: function (data) {
                            angular.forEach(relatedDatamap, function (value, key) {
                                data.resultObject[key] = value;
                            });
                        }
                    };
                    return redirectService.goToApplication(schema.applicationName, "newdetail", params, relatedDatamap);
                }

            };

        }]);

})(angular);