
(function (angular) {
    'use strict';

    function workflowService($http, restService, crudContextHolderService, modalService) {
        var initiateWorkflow = function (schema, datamap, workflowName) {
            var httpParameters = {
                entity: schema.entityName,
                schema: schema.schemaId,
                applicationItemId: datamap["fields"][schema.idFieldName],
                workflowName: workflowName
            };
            //schemaorModalData, datamap, properties, savefn, cancelfn, parentdata, parentschema
            restService.invokePost("Workflow", "InitiateWorkflow", httpParameters, null,
                function(response) {
                    //modalService.show(response.resultObject.schema, {}, {
                    //    title: "Select Workflow", cssclass: "dashboardmodal", onloadfn: function (scope) {
                    //        crudContextHolderService.updateEagerAssociationOptions("workflows", response.resultObject.workflows);
                    //    }
                    //}, null, null, schema, datamap);
                    modalService.show(response.resultObject.schema, {}, {
                        title: "Select Workflow", cssclass: "dashboardmodal", onloadfn: function (scope) {
                            crudContextHolderService.updateEagerAssociationOptions("workflows", response.resultObject.workflows);
                        }
                    }, function (datamap) {
                        var parentDatamap = angular.element(document).injector().get('crudContextHolderService').rootDataMap();
                        var parentSchema = angular.element(document).injector().get('crudContextHolderService').currentSchema();
                        initiateWorkflow(parentSchema, parentDatamap, datamap["processname"]);
                    });
                }, function (response) {
                    // There was an error while trying to initiate workflow
                }
            );
        };

        var service = {
            initiateWorkflow: initiateWorkflow
        };

        return service;

    };

    angular.module('webcommons_services').factory('workflowService', ['$http', 'restService', 'crudContextHolderService', 'modalService', workflowService]);
})(angular);
