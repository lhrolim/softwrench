
(function (angular) {
    'use strict';

    function workflowService($http, restService, crudContextHolderService, modalService, alertService) {
        var initiateWorkflow = function (schema, datamap, workflowName) {
            var httpParameters = {
                entityName: schema.entityName,
                applicationItemId: datamap["fields"][schema.userIdFieldName],
                siteid: datamap["fields"]["siteid"],
                workflowName: workflowName
            };
            //schemaorModalData, datamap, properties, savefn, cancelfn, parentdata, parentschema
            restService.invokePost("Workflow", "InitiateWorkflow", httpParameters, null,
                function (response) {
                    // If the response is null, no workflows were found
                    if (response == "null") {
                        alertService.notifymessage('warn', 'There are no active and enabled Workflows for this record type.');
                    }
                    // If the response does not have a list of workflows, it has sucessfully found and executed one
                    if (!response.resultObject.hasOwnProperty("workflows")) {
                        modalService.hide();
                        return;
                    }
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

    angular.module('webcommons_services').factory('workflowService', ['$http', 'restService', 'crudContextHolderService', 'modalService', 'alertService', workflowService]);
})(angular);
