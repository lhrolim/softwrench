
(function (angular) {
    'use strict';

    function workflowService($q, $http, restService, redirectService, crudContextHolderService, modalService, alertService) {


        var initiateWorkflow = function(schema, datamap, workflowName) {
            var httpParameters = {
                entityName: schema.entityName,
                applicationItemId: datamap["fields"][schema.userIdFieldName],
                siteid: datamap["fields"]["siteid"],
                workflowName: workflowName
            };
            restService.postPromise("Workflow", "InitiateWorkflow", httpParameters).then(function(response) {
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
                    title: "Select Workflow to Initialize",
                    cssclass: "dashboardmodal",
                    onloadfn: function() {
                        crudContextHolderService.updateEagerAssociationOptions("workflows", response.data.resultObject.workflows);
                    }
                }, function(datamap) {
                    var parentDatamap = crudContextHolderService.rootDataMap();
                    var parentSchema = crudContextHolderService.currentSchema();
                    initiateWorkflow(parentSchema, parentDatamap, datamap["processname"]);
                });
            }).catch(function(response) {
                alertService.error(response.data.errorMessage);
            });
        };


        var routeWorkflow = function() {
            var rootDatamap = crudContextHolderService.rootDataMap().fields;
            var schema = crudContextHolderService.currentSchema();


            var httpParameters = {
                entityName: schema.entityName,
                id: rootDatamap[schema.idFieldName],
                appuserId: rootDatamap[schema.userIdFieldName],
                siteid: rootDatamap.siteid,
            };

            return restService.postPromise("Workflow", "InitRouteWorkflow", httpParameters).then(function(response) {
                var appResponse = response.data;
                if (appResponse.errorMessage) {
                    alertService.alert(response.data.errorMessage);
                    return $q.when();
                }

                var deferred = $q.defer();

                if (appResponse.type === "ApplicationDetailResult") {


                    modalService.show(appResponse.schema, appResponse.resultObject.fields, {
                        title: "Complete Workflow Assignment",
                        cssclass: "dashboardmodal",
                        onloadfn: function() {
                            crudContextHolderService.updateEagerAssociationOptions("taskoptions", appResponse.associationOptions.eagerOptions["#taskoptions"]);
                        }
                    }, function(datamap) {
                        var fields = datamap.fields || datamap;
                        var data = {
                            ownerId: rootDatamap[schema.idFieldName],
                            ownerTable: schema.applicationName,
                            appUserId: rootDatamap[schema.userIdFieldName],
                            siteid: rootDatamap["siteid"],
                            wfId: fields["#wfid"],
                            processName: fields["#processName"],
                            memo: fields["#memo"],
                            actionId: fields["#taskoption"],
                            assignmentId: fields["#wfassignmentid"],
                        }
                        restService.postPromise("Workflow", "DoRouteWorkflow", null, data).then(function() {
                            deferred.resolve();
                            modalService.hide();
                        });
                        

                    });
                } else {
                    
                    modalService.show(appResponse.resultObject.schema, appResponse.datamap, {
                        title: "Select Workflow to Route",
                        cssclass: "dashboardmodal",
                        onloadfn: function () {
                            crudContextHolderService.updateEagerAssociationOptions("workflows", appResponse.resultObject.workflows);
                        }
                    }, function (datamap) {
                        var fields = datamap["fields"];
                        var httpParameters = {
                            wfAssignmentId: datamap["processname"],
                        }
                        restService.postPromise("Workflow", "InitRouteWorkflowSelected", httpParameters).then(function() {
                            deferred.resolve();
                            modalService.hide();
                        });
                        
                    });

                }


                return deferred.promise;


            });
        

};

        var stopWorkflow = function (wfInstanceId) {
            var datamap = crudContextHolderService.rootDataMap();
            var schema = crudContextHolderService.currentSchema();
            

            var httpParameters = {
                entityName: schema.entityName,
                id: datamap["fields"][schema.idFieldName],
                userid: datamap["fields"][schema.userIdFieldName],
                siteid: datamap["fields"]["siteid"],
                wfInstanceId: wfInstanceId
            };

            return restService.postPromise("Workflow", "StopWorkflow", httpParameters).then(function(response) {
                var appResponse = response.data;
                if (appResponse.errorMessage) {
                    alertService.alert(response.data.errorMessage);
                    return $q.when();
                }

                // If the response does not have a list of workflows, it has sucessfully found and executed one

                if (appResponse.resultObject) {
                    modalService.show(response.data.resultObject.schema, {}, {
                        title: "Select Workflow to Stop",
                        cssclass: "dashboardmodal",
                        onloadfn: function() {
                            crudContextHolderService.updateEagerAssociationOptions("workflows", response.data.resultObject.workflows);
                        }
                    }, function(datamap) {
                        return stopWorkflow(datamap["processname"]);
                    });
                } else {
                    //closing the modal
                    return $q.when();
                }

            });

        }

        var service = {
            initiateWorkflow: initiateWorkflow,
            stopWorkflow: stopWorkflow,
            routeWorkflow : routeWorkflow
        };

        return service;

    };

    angular.module('webcommons_services').factory('workflowService', ["$q",'$http', 'restService','redirectService', 'crudContextHolderService', 'modalService', 'alertService', workflowService]);
})(angular);
