
(function (angular) {
    "use strict";

    function crudContextHolderService($rootScope, $log,$injector, $timeout, contextService,schemaCacheService) {

        //#region private variables



        //TODO: continue implementing this methods, removing crud_context object references from the contextService
        // ReSharper disable once InconsistentNaming
        var _originalContext = {
            currentSchema: null,
            rootDataMap: null,
            currentApplicationName: null,
            //a datamap that could be used in a transient fashion but that would have the same lifecycle as the main one, i.e, would get cleaned automatically upon app redirect
            auxDataMap:null,
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
            detailDataResolved: false,
            needsServerRefresh: false,
            //list of profiles to show on screen, when there are multiple whereclauses registered for a given grid
            affectedProfiles: [],
            //current profile selected, if multiple are available, considering whereclauses
            currentSelectedProfile: null,
            tabRecordCount: {},
            compositionLoadComplete: false,
            gridSelectionModel: {
                selectionBuffer: {}, // buffer of all selected row
                selectionBufferIdCollumn: null, // preset of columns name to be used as buffer key
                onPageSelectedCount: 0, // number of selected rows on current page
                pageSize: 0, // number of rows of page (no same of pagination on show only selected)
                selectAllValue: false, // whether or not select all checkbox is selected
                showOnlySelected: false,
                selectionMode:false
            },
            commandsModel: {
                toggleCommands: {}
            },
            // pagination data before the toggle selected
            originalPaginationData: null,
            gridModel: {
                //this is used to set a transient whereclause to the grid that should be appended on all subsequent server calls
                fixedWhereClause: null
            },
            customSaveFn:null

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

        function rootDataMap(panelid,datamap) {
            var context = getContext(panelid);
            if (datamap) {
                context.rootDataMap = datamap;
            }
            return context.rootDataMap;
        }

        function getAffectedProfiles(panelid) {
            return getContext(panelid).affectedProfiles || [];
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

        function setTabRecordCount(tabId, panelId,count) {
            var context = getContext(panelId);
            if (context.tabRecordCount && context.tabRecordCount[tabId]) {
                context.tabRecordCount[tabId] = count;
            }
        }
        
        function setDetailDataResolved(panelid) {
            getContext(panelid).detailDataResolved = true;
        }

        function clearDetailDataResolved(panelid) {
            getContext(panelid).detailDataResolved = false;
        }

        function getDetailDataResolved(panelid) {
            return getContext(panelid).detailDataResolved;
        }

        //#endregion

        //#region hooks
        function updateCrudContext(schema, rootDataMap, panelid) {
            var context = getContext(panelid);
            schema.properties = schema.properties || {};
            context.currentSchema = schema;
            context.rootDataMap = rootDataMap;
            context.currentApplicationName = schema.applicationName;
            context.gridSelectionModel.selectionMode = "true" === schema.properties["list.selectionmodebydefault"];
            context.gridSelectionModel.selectionBufferIdCollumn = context.gridSelectionModel.selectionBufferIdCollumn || schema.idFieldName;
            schemaCacheService.addSchemaToCache(schema);
        }

        function applicationChanged(schema, rootDataMap, panelid) {
            this.clearCrudContext(panelid);
            this.updateCrudContext(schema, rootDataMap, panelid);
            $rootScope.$broadcast("sw.crud.applicationchanged", schema, rootDataMap, panelid);
        }

        function clearCrudContext(panelid) {
            if (!panelid) {
                _crudContext = angular.copy(_originalContext);
                return _crudContext;
            }
            _crudContexts[panelid] = angular.copy(_originalContext);
            return _crudContext[panelid];
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
            clearDetailDataResolved(panelid);
            context.tabRecordCount = {};
            context._eagerassociationOptions = { "#global": {} };
            _crudContext._lazyAssociationOptions = {};
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

            var lazyAssociationOptions = _crudContext._lazyAssociationOptions[associationKey];
            if (lazyAssociationOptions == null) {
                log.debug("creating lazy option(s) to association {0}. size: {1}".format(associationKey, length));
                _crudContext._lazyAssociationOptions[associationKey] = options;
            } else {
                log.debug("appending new option(s) to association {0}. size: {1} ".format(associationKey, length));
                _crudContext._lazyAssociationOptions[associationKey] = angular.extend(lazyAssociationOptions, options);
            }
            //to avoid circular dependency, cannot inject it
            var fieldService = $injector.get("fieldService");
            var displayables = fieldService.getDisplayablesByAssociationKey(_crudContext.currentSchema, associationKey);
            if (displayables && displayables.length === 1 && options) {
                //when we have a reverse relationship, let´s add it to the parentdatamap, to make "life" easier for the outer components, such as the angulartypeahead, 
                //and/or expressions
                var displayable = displayables[0];
                var key = Object.keys(options)[0];
                if (displayable.reverse && key) {
                    //Object.keys(options)[0] --> this would be the key of the association
                    _crudContext.rootDataMap.fields[displayable.target] = key.toLowerCase();
                }
            }


        }

        function fetchLazyAssociationOption(associationKey, key, panelid) {
            var associationOptions = _crudContext._lazyAssociationOptions[associationKey];
            if (associationOptions == null) {
                return null;
            }
            var keyToUse = angular.isString(key) ? key.toLowerCase() : key;
            return associationOptions[keyToUse];
        }

        function fetchEagerAssociationOptions(associationKey, contextData, panelid) {
            var context = getContext(panelid);
            //if (context.showingModal) {
            //    contextData = { schemaId: "#modal" };
            //}

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
            if (options == null) {
                //case for dependant associations
                return;
            }

            var context = getContext(panelid);
            if (context.showingModal) {
                contextData = contextData || {};
                contextData.schemaId = "#modal";
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
            clearCrudContext("#modal");
        };

        function modalLoaded(datamap) {
            _crudContext.showingModal = true;
            rootDataMap("#modal", datamap);
        }

        function isShowingModal() {
            return _crudContext.showingModal;
        }

        function registerSaveFn(saveFn) {
            getContext("#modal").customSaveFn = saveFn;
        };

        function getSaveFn() {
            return getContext("#modal").customSaveFn;
        };
        



        //#endregion

        //#region selectionService



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


        function getSelectionModel(panelid) {
            var context = getContext(panelid);
            return context.gridSelectionModel;
        }

        function toggleSelectionMode(panelid) {
            var context = getContext(panelid);
            context.gridSelectionModel.selectionMode = !context.gridSelectionModel.selectionMode;
            return context.gridSelectionModel.selectionMode;
        }

        function getOriginalPaginationData(panelid) {
            return getContext(panelid).originalPaginationData;
        }

        function setOriginalPaginationData(paginationData, panelid) {
            getContext(panelid).originalPaginationData = angular.copy(paginationData);
        }
        //#endregion

        //#region gridServices

        function setFixedWhereClause(panelId,fixedWhereClause) {
            var context = getContext(panelId);
            context.gridModel.fixedWhereClause = fixedWhereClause;
        }

        function getFixedWhereClause(panelId) {
            var context = getContext(panelId);
            return context.gridModel.fixedWhereClause;
        }

        //#endregion

        //#region commandsServices

        function getCommandsModels(panelid) {
            return getContext(panelid).commandsModel;
        }

        function getToggleCommand(commandId, panelid) {
            return getCommandsModels(panelid).toggleCommands[commandId];
        }

        function addToggleCommand(command, panelid) {
            return getCommandsModels(panelid).toggleCommands[command.id] = command;
        }

        //#endregion

        //#region Service Instance

        var service = {
            getAffectedProfiles: getAffectedProfiles,
            getActiveTab: getActiveTab,
            setActiveTab: setActiveTab,
            getTabRecordCount: getTabRecordCount,
            setTabRecordCount: setTabRecordCount,
            shouldShowRecordCount: shouldShowRecordCount,
            getCurrentSelectedProfile: getCurrentSelectedProfile,
            setCurrentSelectedProfile: setCurrentSelectedProfile,
            currentSchema: currentSchema,
            currentApplicationName: currentApplicationName,
            updateCrudContext: updateCrudContext,
            applicationChanged: applicationChanged,
            setDirty: setDirty,
            getDirty: getDirty,
            clearDirty: clearDirty,
            clearCrudContext: clearCrudContext,
            needsServerRefresh: needsServerRefresh,
            rootDataMap: rootDataMap,
            setDetailDataResolved: setDetailDataResolved,
            getDetailDataResolved: getDetailDataResolved,
            clearDetailDataResolved: clearDetailDataResolved
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
            getSaveFn: getSaveFn,
            isShowingModal: isShowingModal,
            modalLoaded: modalLoaded,
            registerSaveFn: registerSaveFn
        }

        var gridServices = {
            setFixedWhereClause: setFixedWhereClause,
            getFixedWhereClause: getFixedWhereClause,
        }

        var selectionService = {
            addSelectionToBuffer: addSelectionToBuffer,
            clearSelectionBuffer: clearSelectionBuffer,
            getOriginalPaginationData: getOriginalPaginationData,
            getSelectionModel: getSelectionModel,
            removeSelectionFromBuffer: removeSelectionFromBuffer,
            setOriginalPaginationData: setOriginalPaginationData,
            toggleSelectionMode: toggleSelectionMode,
            toggleShowOnlySelected: toggleShowOnlySelected,
        }

        var commandsService = {
            getCommandsModel: getCommandsModels,
            getToggleCommand: getToggleCommand,
            addToggleCommand: addToggleCommand
        }

        return angular.extend({}, service, hookServices, associationServices, modalService, selectionService, gridServices, commandsService);


        //#endregion
    }


    angular.module("sw_layout").factory("crudContextHolderService", ["$rootScope", "$log", "$injector", "$timeout", "contextService", "schemaCacheService", crudContextHolderService]);



})(angular);
