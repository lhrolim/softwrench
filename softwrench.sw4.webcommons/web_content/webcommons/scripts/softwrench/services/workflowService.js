
(function (angular) {
    'use strict';

    function workflowService($http, restService) {
        var initiateWorkflow = function (schema, datamap) {
            var httpParameters = {
                entity: schema.entityName,
                applicationItemId: datamap["fields"][schema.idFieldName],
                workflowName: null
            };

            restService.invokePost("Workflow", "InitiateWorkflow", httpParameters, null, null, function () {
                
            });
        };

        var service = {
            initiateWorkflow: initiateWorkflow
        };

        return service;

    };

    angular.module('webcommons_services').factory('workflowService', ['$http', 'restService', workflowService]);
})(angular);
