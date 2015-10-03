
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
            isDirty: false,
            needsServerRefresh: false,
            //list of profiles to show on screen, when there are multiple whereclauses registered for a given grid
            affectedProfiles: [],
            //current profile selected, if multiple are available, considering whereclauses
            currentSelectedProfile:null
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

        function afterSave() {
            this.clearDirty();
            _crudContext.needsServerRefresh = true;
        }

        function detailLoaded() {
            this.clearDirty();
            _crudContext.needsServerRefresh = false;
        }

        function gridLoaded(applicationListResult) {
            _crudContext.affectedProfiles = applicationListResult.affectedProfiles;
            _crudContext.currentSelectedProfile = applicationListResult.currentSelectedProfile;
        }

        function getAffectedProfiles() {
            return _crudContext.affectedProfiles;
        }

        function getCurrentSelectedProfile() {
            return _crudContext.currentSelectedProfile;
        }

        function setCurrentSelectedProfile(currentProfile) {
            return _crudContext.currentSelectedProfile = currentProfile;
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

        function needsServerRefresh() {
            return _crudContext.needsServerRefresh;
        }

        //#endregion

        //#region Service Instance

        var service = {
            getAffectedProfiles: getAffectedProfiles,
            getActiveTab: getActiveTab,
            setActiveTab: setActiveTab,
            getCurrentSelectedProfile:getCurrentSelectedProfile,
            setCurrentSelectedProfile:setCurrentSelectedProfile,
            currentSchema: currentSchema,
            currentApplicationName: currentApplicationName,
            updateCrudContext: updateCrudContext,
            setDirty: setDirty,
            getDirty: getDirty,
            clearDirty: clearDirty,
            needsServerRefresh: needsServerRefresh,
            afterSave: afterSave,
            detailLoaded: detailLoaded,
            gridLoaded: gridLoaded
            
        };

        return service;

      

        //#endregion
    }


    angular.module("sw_layout").factory("crudContextHolderService", ["contextService", "schemaCacheService", crudContextHolderService]);



})(angular);
