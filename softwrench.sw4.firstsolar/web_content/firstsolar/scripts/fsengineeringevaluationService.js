(function (angular, $) {
    'use strict';

    function fsengineeringevaluationService($rootScope, $http, $q, $timeout, modalService, schemaCacheService, crudContextHolderService, submitService, fieldService) {

        const woDetailSchema = "workpackagesimplecomposition";

        function buildDatamap(schema) {
            const datamap = {};
            schema.displayables.forEach(d => {
                if (d.isHidden) {
                    return;
                }
                if (d.target) {
                    datamap[d.target] = null;
                } else {
                    datamap[d.attribute] = null;
                }
            });
            return datamap;
        }

        function getWoDatamap(woId) {
            const params = {
                id: woId,
                key: { schemaId: woDetailSchema, mode: "input", platform: "web" },
                customParameters: {},
                printMode: null
            }
            const urlToCall = url("/api/data/workorder?" + $.param(params));
            return $http.get(urlToCall).then(response => {
                return response.data.resultObject;
            });
        }

        function getWoSchema() {
            return schemaCacheService.fetchSchema("workorder", woDetailSchema);
        }

        function submit(saveDatamap, successMessage) {
            const woId = crudContextHolderService.rootDataMap()["#workorder_.workorderid"];
            const promises = [];
            saveDatamap["#isDirty"] = true;
            saveDatamap["application"] = "worklog";
            promises.push(getWoSchema());
            promises.push(getWoDatamap(woId));
            return $q.all(promises).then((results) => {
                const woDatamap = results[1];
                woDatamap["worklog_"] = [saveDatamap];
                const params = {
                    compositionData: new CompositionOperation("crud_update", "worklog_", saveDatamap, saveDatamap["worklogid"]),
                    dispatchedByModal: false,
                    originalDatamap: woDatamap,
                    refresh: true,
                    successMessage: successMessage
                }
                return submitService.submit(results[0], woDatamap, params);
            });
        }

        function sendEvaluationEmail(evaluationDm, relationship) {
            const rootDm = crudContextHolderService.rootDataMap();
            const schema = crudContextHolderService.currentSchema();

            const evaluationField = fieldService.getDisplayableByKey(schema, relationship);
            const testSection = evaluationField ? fieldService.getDisplayableByKey(schema, evaluationField.parentsectionid) : null;
            const testName = testSection ? testSection.header.label : "";

            const sendEmailParams = {
                wpId: rootDm["id"],
                siteId: rootDm["#workorder_.siteid"],
                evaluation: evaluationDm["wld_.ldtext"],
                testName: testName
            }
            const sendEmailurlToCall = url("/firstSolarEmail/EngeneeringEvaluationEmail");
            return $http.post(sendEmailurlToCall, JSON.stringify(sendEmailParams));
        }

        function openModalNew(item, callbackAdd, rollbackAdd, callbackSave, relationship) {
            schemaCacheService.fetchSchema("worklog", "workpackagenewdetail").then((schema) => {
                const datamap = buildDatamap(schema);
                modalService.show(schema, datamap,{
                        cssclass: 'largemodal',
                        removecrudmodalclass: true,
                        resizable: true,
                        resizableElements: ' #crudmodal iframe '
                    }, (saveDatamap) => {


                        saveDatamap["_iscreation"] = true;
                        saveDatamap["clientviewable"] = 0;
                        saveDatamap["description"] = `swwpkg:${relationship.substr(1, relationship.length - 3)}`;
                        saveDatamap["logtype"] = "CLIENTNOTE";
                        saveDatamap["worklogid"] = undefined;
                        submit(saveDatamap, "Evaluation successfully created.").then(data => {
                            modalService.hide();
                            callbackSave(data, false, true);

                            // send evaluation email
                            return sendEvaluationEmail(saveDatamap, relationship);
                        });
                    });
            });
        }

        function openModalEdit(item, callbackAdd, rollbackAdd, callbackSave, relationship) {
            schemaCacheService.fetchSchema("worklog", "workpackagedetail").then((schema) => {
                const datamap = angular.copy(item);
                modalService.show(schema, datamap, { cssclass: 'largemodal', removecrudmodalclass: true, resizable: true, resizableElements: ' #crudmodal iframe ' }, (saveDatamap) => {
                    $("#crudmodal iframe").height('800px');
                    submit(saveDatamap, "Evaluation successfully updated.").then(data => {
                        modalService.hide();
                        callbackSave(data, false, true);
                    });
                });
            });
        }

        const service = {
            openModalNew,
            openModalEdit
        };
        return service;
    }

    angular
        .module("firstsolar")
        .clientfactory("fsengineeringevaluationService", ["$rootScope", "$http", "$q", "$timeout", "modalService", "schemaCacheService", "crudContextHolderService", "submitService", "fieldService", fsengineeringevaluationService]);
})(angular, jQuery);