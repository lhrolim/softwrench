(function (angular) {
    'use strict';
    
    function fsworkpackagefilesService($http, $q, schemaCacheService, crudContextHolderService, submitService, compositionService, searchService) {

        const woDetailSchema = "editdetail";

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
                    document: `swwpkg:${relationship.substr(1, relationship.length - 2)}`,
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
                    file.persisted = true;
                    const packageDatamap = crudContextHolderService.rootDataMap();
                    const packageSchema = crudContextHolderService.currentSchema();
                    crudContextHolderService.clearCompositionsLoaded();
                    return compositionService.searchCompositionList(relationship, packageSchema, packageDatamap);
                });
            });
        }

        const service = {
            saveFile
        };
        return service;
    }

    angular
    .module("firstsolar")
        .clientfactory("fsworkpackagefilesService", ["$http", "$q", "schemaCacheService", "crudContextHolderService", "submitService", "compositionService", "searchService", fsworkpackagefilesService]);
})(angular);