
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
            restService.postPromise("Workflow", "InitiateWorkflow", httpParameters).then(function (response) {
                // If the response is null, no workflows were found
                if (response.data == "null") {
                    alertService.notifymessage('warn', 'There are no active and enabled Workflows for this record type.');
                }
                // If the response does not have a list of workflows, it has sucessfully found and executed one
                if (!response.data.resultObject.hasOwnProperty("workflows")) {
                    modalService.hide();
                    return;
                }
                modalService.show(response.data.resultObject.schema, {}, {
                    title: "Select Workflow", cssclass: "dashboardmodal", onloadfn: function () {
                        crudContextHolderService.updateEagerAssociationOptions("workflows", response.data.resultObject.workflows);
                    }
                }, function (datamap) {
                    var parentDatamap = angular.element(document).injector().get('crudContextHolderService').rootDataMap();
                    var parentSchema = angular.element(document).injector().get('crudContextHolderService').currentSchema();
                    initiateWorkflow(parentSchema, parentDatamap, datamap["processname"]);
                });
            }).catch(function (response) {
                alertService.error(response.data.errorMessage);
            });
        };

        var service = {
            initiateWorkflow: initiateWorkflow
        };

        return service;

    };

    angular.module('webcommons_services').factory('workflowService', ['$http', 'restService', 'crudContextHolderService', 'modalService', 'alertService', workflowService]);
})(angular);
