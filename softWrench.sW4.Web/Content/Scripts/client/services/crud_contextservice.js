
(function (angular) {
    "use strict";

    function crudContextHolderService($rootScope, $log, $timeout, contextService, schemaCacheService) {

        //#region private variables



        //TODO: continue implementing this methods, removing crud_context object references from the contextService
        // ReSharper disable once InconsistentNaming
        var _originalContext = {
            currentSchema: null,
            rootDataMap:null,
            currentApplicationName: null,

            /*{
            #global:{
             eagerassociation1_ :[],
             eagerassociation2_ :[],
            },
            
             composition1_ : {
                    #global:{
                    eagerassociation1_:[],
                    eagerassociation2_:[],
                    }
                    'itemid1':{
                        eagerassociation3_:[],
                        eagerassociation4_:[],
                    },
                    'itemid2':{
                        eagerassociation3_:[],
                        eagerassociation4_:[],
                    }
                }
            }*/

            _eagerassociationOptions: {
                "#global": {}
            },

            ///
            ///  asset_:{
            //      '1': {code:'1',description:'100'},  
            //      '2': {code:'2',description:'2'},  
            //   },
            //
            //  owner_:{
            //      '1': {code:'1',description:'100'},  
            //      '2': {code:'2',description:'2'},  
            //   },
            ///
            ///
            ///
            ///
            ///
            _lazyAssociationOptions: {},
            /**
             * asset_
             * 
             */
            _blockedAssociations: {},

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
            currentSelectedProfile: null,
            tabRecordCount: {},
            compositionLoadComplete: false,
        };

        var _crudContext = angular.copy(_originalContext);



        //#endregion

        //#region Public methods

        //#region simple getters/setters

        function getActiveTab() {
            return contextService.getActiveTab();
        }

        function setActiveTab(tabId) {
            contextService.setActiveTab(tabId);

            //update header/footer layout
            $timeout(function () {
                $(window).trigger('resize');
            }, false);
        }

        function currentApplicationName() {
            return _crudContext.currentApplicationName;
        }

        function currentSchema() {
            return _crudContext.currentSchema;
        }

        function rootDataMap() {
            return _crudContext.rootDataMap;
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


        function getTabRecordCount(tab) {
            if (_crudContext.tabRecordCount && _crudContext.tabRecordCount[tab.tabId]) {
                return _crudContext.tabRecordCount[tab.tabId];
            }
            return 0;
        }

        function shouldShowRecordCount(tab) {
            return _crudContext.tabRecordCount && _crudContext.tabRecordCount[tab.tabId];
        }


        //#endregion

        //#region hooks
        function updateCrudContext(schema,rootDataMap) {
            //            _crudContext = {};
            _crudContext.currentSchema = schema;
            _crudContext.rootDataMap = rootDataMap;
            _crudContext.currentApplicationName = schema.applicationName;
            schemaCacheService.addSchemaToCache(schema);
        }

        function clearCrudContext() {
            _crudContext = angular.copy(_originalContext);
        }

        function afterSave() {
            this.clearDirty();
            _crudContext.needsServerRefresh = true;
        }

        function detailLoaded() {
            this.clearDirty();
            this.disposeDetail();
            _crudContext.needsServerRefresh = false;

        }

        function gridLoaded(applicationListResult) {
            this.disposeDetail();
            this.setActiveTab(null);
            _crudContext.affectedProfiles = applicationListResult.affectedProfiles;
            _crudContext.currentSelectedProfile = applicationListResult.currentSelectedProfile;
        }

        function disposeDetail() {
            _crudContext.tabRecordCount = {};
            _crudContext._eagerassociationOptions = { "#global": {} };
            _crudContext._lazyAssociationOptions = {};
            _crudContext.compositionLoadComplete = false;
            _crudContext.associationsResolved = false;
            contextService.setActiveTab(null);
        }

        function compositionsLoaded(result) {
            for (var relationship in result) {
                var tab = result[relationship];
                _crudContext.tabRecordCount = _crudContext.tabRecordCount || {};
                _crudContext.tabRecordCount[relationship] = tab.paginationData.totalCount;
            }
            _crudContext.compositionLoadComplete = true;
        }



        //#endregion

        //#region associations

        /**
         * 
         * @param {} associationKey 
         * @param {} options 
         * @param {} notIndexed  if true we need to transform the options object to an indexed version in advance ( {value:xxx, label:yyy} --> {xxx:{value:xxx, label:yyy}}
         * @returns {} 
         */
        function updateLazyAssociationOption(associationKey, options, notIndexed) {
            var log = $log.get("crudcontextHolderService#updateLazyAssociationOption", ["association"]);
            if (!!notIndexed && options != null) {
                var objIdxKey = options.value.toLowerCase();
                var idxedObject = {};
                idxedObject[objIdxKey] = options;
                options = idxedObject;
            }
            var length = "null";
            if (options) {
                length = options.length ? options.length : 1;
            }

            var lazyAssociationOptions = _crudContext._lazyAssociationOptions[associationKey];
            if (lazyAssociationOptions == null) {
                log.debug("creating lazy option(s) to association {0}. size: {1}".format(associationKey, length));
                _crudContext._lazyAssociationOptions[associationKey] = options;
            } else {
                log.debug("appending new option(s) to association {0}. size: {1} ".format(associationKey, length));
                _crudContext._lazyAssociationOptions[associationKey] = angular.extend(lazyAssociationOptions, options);
            }
        }

        function fetchLazyAssociationOption(associationKey, key) {
            var associationOptions = _crudContext._lazyAssociationOptions[associationKey];
            if (associationOptions == null) {
                return null;
            }
            var keyToUse = angular.isString(key) ? key.toLowerCase() : key;
            return associationOptions[keyToUse];
        }

        function fetchEagerAssociationOptions(associationKey, contextData) {

            if (contextData == null) {
                return _crudContext._eagerassociationOptions["#global"][associationKey];
            }

            var schemaId = contextData.schemaId;
            var entryId = contextData.entryId || "#global";

            _crudContext._eagerassociationOptions[schemaId] = _crudContext._eagerassociationOptions[schemaId] || {};
            _crudContext._eagerassociationOptions[schemaId][entryId] = _crudContext._eagerassociationOptions[schemaId][entryId] || {};

            return _crudContext._eagerassociationOptions[schemaId][entryId][associationKey];
        }


        function updateEagerAssociationOptions(associationKey, options, contextData) {
            if (_crudContext.showingModal) {
                contextData = { schemaId: "#modal" };
            }
            var log = $log.getInstance("crudContext#updateEagerAssociationOptions", ["association"]);

            

            if (contextData == null) {
                log.info("update eager global list for {0}. Size: {1}".format(associationKey, options.length));
                _crudContext._eagerassociationOptions["#global"][associationKey] = options;
                $rootScope.$broadcast("sw.crud.associations.updateeageroptions", associationKey, options, contextData);
                return;
            }

            var schemaId = contextData.schemaId;
            var entryId = contextData.entryId || "#global";

            _crudContext._eagerassociationOptions[schemaId] = _crudContext._eagerassociationOptions[schemaId] || {};
            _crudContext._eagerassociationOptions[schemaId][entryId] = _crudContext._eagerassociationOptions[schemaId][entryId] || {};

            _crudContext._eagerassociationOptions[schemaId][entryId][associationKey] = options;

            log.info("update eager list for {0}. Size: {1}".format(associationKey, options.length));

            $rootScope.$broadcast("sw.crud.associations.updateeageroptions", associationKey, options, contextData);


        }

        function markAssociationsResolved() {
            _crudContext.associationsResolved = true;
            $rootScope.$broadcast("sw_associationsresolved");
        }

        function associationsResolved() {
            return _crudContext.associationsResolved;
        }


        //#endregion
        //#endregion

        //#region modal

        function disposeModal() {
            _crudContext.showingModal = false;
            _crudContext._eagerassociationOptions["#modal"] = { "#global": {} };
        };

        function modalLoaded() {
            _crudContext.showingModal = true;
        }

        //#endregion


        //#region Service Instance

        var service = {
            getAffectedProfiles: getAffectedProfiles,
            getActiveTab: getActiveTab,
            setActiveTab: setActiveTab,
            getTabRecordCount: getTabRecordCount,
            shouldShowRecordCount: shouldShowRecordCount,
            getCurrentSelectedProfile: getCurrentSelectedProfile,
            setCurrentSelectedProfile: setCurrentSelectedProfile,
            currentSchema: currentSchema,
            currentApplicationName: currentApplicationName,
            updateCrudContext: updateCrudContext,
            setDirty: setDirty,
            getDirty: getDirty,
            clearDirty: clearDirty,
            clearCrudContext: clearCrudContext,
            needsServerRefresh: needsServerRefresh,
            rootDataMap: rootDataMap
        };

        var associationServices = {
            updateLazyAssociationOption: updateLazyAssociationOption,
            fetchLazyAssociationOption: fetchLazyAssociationOption,
            updateEagerAssociationOptions: updateEagerAssociationOptions,
            fetchEagerAssociationOptions: fetchEagerAssociationOptions,
            associationsResolved: associationsResolved,
            markAssociationsResolved: markAssociationsResolved,

        }

        var hookServices = {
            afterSave: afterSave,
            detailLoaded: detailLoaded,
            disposeDetail: disposeDetail,
            gridLoaded: gridLoaded,
            compositionsLoaded: compositionsLoaded
        }

        var modalService = {
            disposeModal: disposeModal,
            modalLoaded: modalLoaded
        }


        return angular.extend({}, service, hookServices, associationServices, modalService);


        //#endregion
    }


    angular.module("sw_layout").factory("crudContextHolderService", ['$rootScope', "$log", "$timeout", "contextService", "schemaCacheService", crudContextHolderService]);



})(angular);
