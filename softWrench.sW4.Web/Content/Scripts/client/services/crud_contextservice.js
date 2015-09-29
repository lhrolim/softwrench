﻿
(function (angular) {
    "use strict";

  
    function crudContextHolderService(contextService, schemaCacheService) {

        //#region Utils



        //TODO: continue implementing this methods, removing crud_context object references from the contextService
        // ReSharper disable once InconsistentNaming
        var _crudContext = {
            currentSchema: null,
            currentApplicationName: null,
            //TODO: below is yet to be implemented/refactored
            detail_previous: "0",
            detail_next: "0",
            list_elements: [],
            previousData: null,
            paginationData: null,
            isDirty :false
        };

        //#endregion

        //#region Public methods

        function getActiveTab() {
            return contextService.getActiveTab();
        }

        function setActiveTab(tabId) {
            contextService.setActiveTab(tabId);
        }

        function currentApplicationName() {
            return _crudContext.currentApplicationName;
        }

        function currentSchema() {
            return _crudContext.currentSchema;
        }


        function updateCrudContext(schema) {
            _crudContext = {};
            _crudContext.currentSchema = schema;
            _crudContext.currentApplicationName = schema.applicationName;
            schemaCacheService.addSchemaToCache(schema);
        }

        function setDirty() {
            _crudContext.isDirty = true;
        };

        function getDirty() {
            return _crudContext.isDirty;
        };

        function clearDirty() {
            _crudContext.isDirty = false;
        }


        //#endregion

        //#region Service Instance

        var service = {
            getActiveTab: getActiveTab,
            setActiveTab: setActiveTab,
            currentSchema: currentSchema,
            currentApplicationName: currentApplicationName,
            updateCrudContext: updateCrudContext,
            setDirty: setDirty,
            getDirty: getDirty,
            clearDirty: clearDirty
        };

        return service;

      

        //#endregion
    }


    angular.module("sw_layout").factory("crudContextHolderService", ["contextService", "schemaCacheService", crudContextHolderService]);



})(angular);
