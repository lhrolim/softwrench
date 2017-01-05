
(function (angular) {
    'use strict';

    function workflowService($q, $http, restService, redirectService, crudContextHolderService, modalService, alertService) {


        var initiateWorkflow = function (schema, datamap, workflowName) {
            const fields = datamap;
            const httpParameters = {
                appName: schema.applicationName,
                appId: fields[schema.idFieldName],
                appUserId: fields[schema.userIdFieldName],
                siteid: fields["siteid"],
                orgid: fields["orgid"],
                workflowName: workflowName
            };
            return restService.postPromise("Workflow", "InitiateWorkflow", httpParameters).then(function (response) {
                const appResponse = response.data;
                if (appResponse.errorMessage) {
                    alertService.alert(appResponse.errorMessage);
                    return $q.when();
                }

                // If the response does not have a list of workflows, it has successfully found and executed one
                if (appResponse.resultObject) {
                    modalService.show(response.data.resultObject.schema, {}, {
                        title: "Select Workflow to Initialize",
                        cssclass: "dashboardmodal",
                        onloadfn: function() {
                            crudContextHolderService.updateEagerAssociationOptions("workflows", response.data.resultObject.workflows);
                        }
                    }, function(datamap) {
                        const parentDatamap = crudContextHolderService.rootDataMap();
                        const parentSchema = crudContextHolderService.currentSchema();
                        return initiateWorkflow(parentSchema, parentDatamap, datamap["processname"]);
                    });
                } else {
                    return $q.when();
                }
                
            });
        };
        const routeWorkflow = function () {
            var rootDatamap = crudContextHolderService.rootDataMap();
            var schema = crudContextHolderService.currentSchema();
            const httpParameters = {
                entityName: schema.entityName,
                id: rootDatamap[schema.idFieldName],
                appuserId: rootDatamap[schema.userIdFieldName],
                siteid: rootDatamap.siteid,
            };
            return restService.postPromise("Workflow", "InitRouteWorkflow", httpParameters).then(function (response) {
                var appResponse = response.data;
                if (appResponse.errorMessage) {
                    alertService.alert(response.data.errorMessage);
                    return $q.when();
                }

                var deferred = $q.defer();

                var CompleteSelectedWFAssignment = function (appResponse) {
                    modalService.show(appResponse.schema, appResponse.resultObject, {
                        title: "Complete Workflow Assignment",
                        cssclass: "dashboardmodal",
                        onloadfn: function () {
                            crudContextHolderService.updateEagerAssociationOptions("taskoptions", appResponse.associationOptions.eagerOptions["#taskoptions"]);
                        }
                    }, function (datamap) {
                        const fields = datamap;
                        const data = {
                            ownerId: rootDatamap[schema.idFieldName],
                            ownerTable: schema.applicationName,
                            appUserId: rootDatamap[schema.userIdFieldName],
                            siteid: rootDatamap["siteid"],
                            orgId: rootDatamap["orgid"],
                            wfId: fields["#wfid"],
                            processName: fields["#processName"],
                            memo: fields["#memo"],
                            actionId: fields["#taskoption"],
                            assignmentId: fields["#wfassignmentid"],
                        };
                        restService.postPromise("Workflow", "DoRouteWorkflow", null, data).then(function () {
                            deferred.resolve();
                            modalService.hide();
                        });


                    });
                }

                if (appResponse.type === "ApplicationDetailResult") {
                    CompleteSelectedWFAssignment(appResponse);
                } else {

                    modalService.show(appResponse.resultObject.schema, appResponse.datamap, {
                        title: "Select Workflow to Route",
                        cssclass: "dashboardmodal",
                        onloadfn: function () {
                            crudContextHolderService.updateEagerAssociationOptions("workflows", appResponse.resultObject.workflows);
                        }
                    }, function (datamap) {
                        const httpParameters = {
                            wfAssignmentId: datamap["processname"],
                        };
                        restService.postPromise("Workflow", "InitRouteWorkflowSelected", httpParameters).then(function (response) {
                            deferred.resolve();
                            CompleteSelectedWFAssignment(response.data);
                        });

                    });

                }


                return deferred.promise;


            });


        };
        var stopWorkflow = function (wfInstanceId) {
            const datamap = crudContextHolderService.rootDataMap();
            const schema = crudContextHolderService.currentSchema();
            const httpParameters = {
                entityName: schema.entityName,
                id: datamap[schema.idFieldName],
                userid: datamap[schema.userIdFieldName],
                siteid: datamap["siteid"],
                orgid: datamap["orgid"],
                wfInstanceId: wfInstanceId
            };
            return restService.postPromise("Workflow", "StopWorkflow", httpParameters).then(function (response) {
                const appResponse = response.data;
                if (appResponse.errorMessage) {
                    alertService.alert(response.data.errorMessage);
                    return $q.when();
                }

                // If the response does not have a list of workflows, it has successfully found and executed one

                if (appResponse.resultObject) {
                    modalService.show(response.data.resultObject.schema, {}, {
                        title: "Select Workflow to Stop",
                        cssclass: "dashboardmodal",
                        onloadfn: function () {
                            crudContextHolderService.updateEagerAssociationOptions("workflows", response.data.resultObject.workflows);
                        }
                    }, function (datamap) {
                        return stopWorkflow(datamap["processname"]);
                    });
                } else {
                    //closing the modal
                    return $q.when();
                }

            });

        }
        const service = {
            initiateWorkflow: initiateWorkflow,
            stopWorkflow: stopWorkflow,
            routeWorkflow: routeWorkflow
        };
        return service;

    };

    angular.module('webcommons_services').service('workflowService', ["$q", '$http', 'restService', 'redirectService', 'crudContextHolderService', 'modalService', 'alertService', workflowService]);
})(angular);
