(function (angular) {
    'use strict';
    
    function fsworkpackagefilesService($http, $q, schemaCacheService, crudContextHolderService, submitService, compositionService) {

        const woDetailSchema = "workpackagesimplecomposition";

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

        function saveFile(file, relationship) {
            const woId = crudContextHolderService.rootDataMap()["#workorder_.workorderid"];
            const promises = [];
            promises.push(getWoSchema());
            promises.push(getWoDatamap(woId));

            return $q.all(promises).then((results) => {
                const woDatamap = results[1];

                const datamap = {
                    "#isDirty": true,
                    createdate: null,
                    "docinfo_.description": file.label,
                    document: guid().substring(0, 20),
                    "#filter": `swwpkg:${relationship.substr(1, relationship.length - 14)}`,
                    newattachment: file.value,
                    newattachment_path: file.label,
                    _iscreation: true
                }
                woDatamap["attachment_"] = [datamap];

                const params = {
                    compositionData: new CompositionOperation("crud_create", "attachment_", datamap),
                    dispatchedByModal: false,
                    originalDatamap: woDatamap,
                    refresh: true,
                    successMessage: "File successfully saved."
                }
                return submitService.submit(results[0], woDatamap, params).then(data => {
                    const packageDatamap = crudContextHolderService.rootDataMap();
                    const packageSchema = crudContextHolderService.currentSchema();
                    return compositionService.populateWithCompositionData(packageSchema, packageDatamap).then(d => {
                        file.persisted = true;
                        return true;
                    });
                });
            });
        }

        function deleteFile(compositionItem) {
            const woId = crudContextHolderService.rootDataMap()["#workorder_.workorderid"];
            const promises = [];
            promises.push(getWoSchema());
            promises.push(getWoDatamap(woId));

            return $q.all(promises).then((results) => {
                const woDatamap = results[1];

                compositionItem["#deleted"] = 1;
                woDatamap["attachment_"] = [compositionItem];

                const params = {
                    compositionData: new CompositionOperation("crud_delete", "attachment_", compositionItem,compositionItem["doclinksid"]),
                    dispatchedByModal: false,
                    originalDatamap: woDatamap,
                    refresh: true,
                    successMessage: "File successfully deleted."
                }
                return submitService.submit(results[0], woDatamap, params).then(data => {
                    const packageDatamap = crudContextHolderService.rootDataMap();
                    const packageSchema = crudContextHolderService.currentSchema();
                    //do now wait for this promise, so that the file is removed from the screen quickier
                    compositionService.populateWithCompositionData(packageSchema, packageDatamap);
                    return true;
                });
            });
        }

        const service = {
            deleteFile,
            saveFile
        };
        return service;
    }

    angular
    .module("firstsolar")
        .clientfactory("fsworkpackagefilesService", ["$http", "$q", "schemaCacheService", "crudContextHolderService", "submitService", "compositionService", fsworkpackagefilesService]);
})(angular);