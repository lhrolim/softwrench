
(function (angular) {
    "use strict";

    function crudContextHolderService($rootScope, $log, $timeout, contextService, schemaCacheService) {

        //#region private variables



        //TODO: continue implementing this methods, removing crud_context object references from the contextService
        // ReSharper disable once InconsistentNaming
        var _originalContext = {
            currentSchema: null,
            rootDataMap: null,
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
            gridSelectionModel: {
                selectionBuffer: {}, // buffer of all selected row
                onPageSelectedCount: 0, // number of selected rows on current page
                pageSize: 0, // number of rows of page (no same of pagination on show only selected)
                selectAllValue : false, // whether or not select all checkbox is selected
                showOnlySelected: false
            },
            // pagination data before the toggle selected
            originalPaginationData: null
        };

        var _crudContext = angular.copy(_originalContext);

        var _crudContexts = {};

        /**
         * Returns (or create an returns) a context for the given paneilId.
         * Isolates the crud context of distinct panels.
         * All API methods are based on panelId and only affects the specific context.
         * 
         * @param {} panelid 
         * @returns {} 
         */
        var getContext = function (panelid) {
            if (!panelid) {
                return _crudContext;
            }
            var context = _crudContexts[panelid];
            if (!context) {
                _crudContexts[panelid] = angular.copy(_originalContext);
                context = _crudContexts[panelid];
            }
            return context;
        }

        //#endregion

        //#region Public methods

        //#region simple getters/setters

        function getActiveTab() {
            return contextService.getActiveTab();
        }

        function setActiveTab(tabId) {
            contextService.setActiveTab(tabId);

            $timeout(function () {
                $(window).trigger('resize');
            }, false);
        }

        function currentApplicationName(panelid) {
            return getContext(panelid).currentApplicationName;
        }

        function currentSchema(panelid) {
            return getContext(panelid).currentSchema;
        }

        function rootDataMap(panelid) {
            return getContext(panelid).rootDataMap;
        }

        function getAffectedProfiles(panelid) {
            return getContext(panelid).affectedProfiles;
        }

        function getCurrentSelectedProfile(panelid) {
            return getContext(panelid).currentSelectedProfile;
        }

        function setCurrentSelectedProfile(currentProfile, panelid) {
            return getContext(panelid).currentSelectedProfile = currentProfile;
        }

        function setDirty(panelid) {
            getContext(panelid).isDirty = true;
        };

        function getDirty(panelid) {
            return getContext(panelid).isDirty;
        };

        function clearDirty(panelid) {
            getContext(panelid).isDirty = false;
        }

        function needsServerRefresh(panelid) {
            return getContext(panelid).needsServerRefresh;
        }


        function getTabRecordCount(tab, panelid) {
            var context = getContext(panelid);
            if (context.tabRecordCount && context.tabRecordCount[tab.tabId]) {
                return context.tabRecordCount[tab.tabId];
            }
            return 0;
        }

        function shouldShowRecordCount(tab, panelid) {
            var context = getContext(panelid);
            return context.tabRecordCount && context.tabRecordCount[tab.tabId];
        }


        //#endregion

        //#region hooks
        function updateCrudContext(schema, rootDataMap, panelid) {
            var context = getContext(panelid);
            context.currentSchema = schema;
            context.rootDataMap = rootDataMap;
            context.currentApplicationName = schema.applicationName;
            schemaCacheService.addSchemaToCache(schema);
        }

        function applicationChanged(schema, rootDataMap, panelid) {
            this.clearCrudContext(panelid);
            this.updateCrudContext(schema, rootDataMap, panelid);
        }

        function clearCrudContext(panelid) {
            if (!panelid) {
                _crudContext = angular.copy(_originalContext);
            } else {
                _crudContexts[panelid] = angular.copy(_originalContext);
            }
        }

        function afterSave(panelid) {
            this.clearDirty(panelid);
            getContext(panelid).needsServerRefresh = true;
        }

        function detailLoaded(panelid) {
            this.clearDirty(panelid);
            this.disposeDetail(panelid);
            getContext(panelid).needsServerRefresh = false;

        }

        function gridLoaded(applicationListResult, panelid) {
            this.disposeDetail(panelid);
            this.setActiveTab(null, panelid);
            var context = getContext(panelid);
            context.affectedProfiles = applicationListResult.affectedProfiles;
            context.currentSelectedProfile = applicationListResult.currentSelectedProfile;
        }

        function disposeDetail(panelid) {
            var context = getContext(panelid);
            context.tabRecordCount = {};
            context._eagerassociationOptions = { "#global": {} };
            context._lazyAssociationOptions = {};
            context.compositionLoadComplete = false;
            context.associationsResolved = false;
            contextService.setActiveTab(null);
        }

        function compositionsLoaded(result, panelid) {
            var context = getContext(panelid);
            for (var relationship in result) {
                var tab = result[relationship];
                context.tabRecordCount = context.tabRecordCount || {};
                context.tabRecordCount[relationship] = tab.paginationData.totalCount;
            }
            context.compositionLoadComplete = true;
        }



        //#endregion

        //#region associations

        /**
         * 
         * @param {} associationKey 
         * @param {} options 
         * @param {} notIndexed  if true we need to transform the options object to an indexed version in advance ( {value:xxx, label:yyy} --> {xxx:{value:xxx, label:yyy}}
         * @param {} panelid 
         * @returns {} 
         */
        function updateLazyAssociationOption(associationKey, options, notIndexed, panelid) {
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

            var context = getContext(panelid);
            var lazyAssociationOptions = context._lazyAssociationOptions[associationKey];
            if (lazyAssociationOptions == null) {
                log.debug("creating lazy option(s) to association {0}. size: {1}".format(associationKey, length));
                context._lazyAssociationOptions[associationKey] = options;
            } else {
                log.debug("appending new option(s) to association {0}. size: {1} ".format(associationKey, length));
                context._lazyAssociationOptions[associationKey] = angular.extend(lazyAssociationOptions, options);
            }
        }

        function fetchLazyAssociationOption(associationKey, key, panelid) {
            var associationOptions = getContext(panelid)._lazyAssociationOptions[associationKey];
            if (associationOptions == null) {
                return null;
            }
            var keyToUse = angular.isString(key) ? key.toLowerCase() : key;
            return associationOptions[keyToUse];
        }

        function fetchEagerAssociationOptions(associationKey, contextData, panelid) {
            var context = getContext(panelid);
            if (contextData == null) {
                return context._eagerassociationOptions["#global"][associationKey];
            }

            var schemaId = contextData.schemaId;
            var entryId = contextData.entryId || "#global";

            context._eagerassociationOptions[schemaId] = context._eagerassociationOptions[schemaId] || {};
            context._eagerassociationOptions[schemaId][entryId] = context._eagerassociationOptions[schemaId][entryId] || {};

            return context._eagerassociationOptions[schemaId][entryId][associationKey];
        }


        function updateEagerAssociationOptions(associationKey, options, contextData, panelid) {
            var context = getContext(panelid);
            if (context.showingModal) {
                contextData = { schemaId: "#modal" };
            }
            var log = $log.getInstance("crudContext#updateEagerAssociationOptions", ["association"]);



            if (contextData == null) {
                log.info("update eager global list for {0}. Size: {1}".format(associationKey, options.length));
                context._eagerassociationOptions["#global"][associationKey] = options;
                $rootScope.$broadcast("sw.crud.associations.updateeageroptions", associationKey, options, contextData);
                return;
            }

            var schemaId = contextData.schemaId;
            var entryId = contextData.entryId || "#global";

            context._eagerassociationOptions[schemaId] = context._eagerassociationOptions[schemaId] || {};
            context._eagerassociationOptions[schemaId][entryId] = context._eagerassociationOptions[schemaId][entryId] || {};

            context._eagerassociationOptions[schemaId][entryId][associationKey] = options;

            log.info("update eager list for {0}. Size: {1}".format(associationKey, options.length));

            $rootScope.$broadcast("sw.crud.associations.updateeageroptions", associationKey, options, contextData);


        }

        function markAssociationsResolved(panelid) {
            getContext(panelid).associationsResolved = true;
            $rootScope.$broadcast("sw_associationsresolved");
        }

        function associationsResolved(panelid) {
            return getContext(panelid).associationsResolved;
        }


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

        //#region selectionService

        function getSelectionModel(panelid) {
            return getContext(panelid).gridSelectionModel;
        }

        function addSelectionToBuffer(rowId, row, panelid) {
            getContext(panelid).gridSelectionModel.selectionBuffer[rowId] = row;
        }

        function removeSelectionFromBuffer(rowId, panelid) {
            delete getContext(panelid).gridSelectionModel.selectionBuffer[rowId];
        }

        function clearSelectionBuffer(panelid) {
            getContext(panelid).gridSelectionModel.selectionBuffer = {};
        }

        function toggleShowOnlySelected(panelid) {
            var context = getContext(panelid);
            context.gridSelectionModel.showOnlySelected = !context.gridSelectionModel.showOnlySelected;
            return context.gridSelectionModel.showOnlySelected;
        }

        function getOriginalPaginationData(panelid) {
            return getContext(panelid).originalPaginationData;
        }

        function setOriginalPaginationData(paginationData, panelid) {
            getContext(panelid).originalPaginationData = angular.copy(paginationData);
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
            applicationChanged : applicationChanged,
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

        var selectionService = {
            getSelectionModel: getSelectionModel,
            addSelectionToBuffer: addSelectionToBuffer,
            removeSelectionFromBuffer: removeSelectionFromBuffer,
            clearSelectionBuffer: clearSelectionBuffer,
            toggleShowOnlySelected: toggleShowOnlySelected,
            getOriginalPaginationData: getOriginalPaginationData,
            setOriginalPaginationData: setOriginalPaginationData
        }

        return angular.extend({}, service, hookServices, associationServices, modalService, selectionService);


        //#endregion
    }


    angular.module("sw_layout").factory("crudContextHolderService", ['$rootScope', "$log", "$timeout", "contextService", "schemaCacheService", crudContextHolderService]);



})(angular);
