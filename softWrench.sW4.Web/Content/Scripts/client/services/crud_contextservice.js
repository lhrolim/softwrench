
(function (angular) {
    "use strict";

    function crudContextHolderService($rootScope, $log, $injector, $timeout, contextService, schemaCacheService) {

        //#region private variables



        //TODO: continue implementing this methods, removing crud_context object references from the contextService
        // ReSharper disable once InconsistentNaming
        var _originalContext = {
            currentSchema: null,
            rootDataMap: null,
            originalDatamap: null,
            isList: null,
            isDetail: null,
            crudForm:null,

            currentApplicationName: null,
            //a datamap that could be used in a transient fashion but that would have the same lifecycle as the main one, i.e, would get cleaned automatically upon app redirect
            auxDataMap: null,
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

            compositionLoadEventQueue: {},

            //TODO: below is yet to be implemented/refactored
            detail_previous: { id: "0" },
            detail_next: { id: "0" },
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
                selectionMode: false
            },
            commandsModel: {
                toggleCommands: {}
            },
            // pagination data before the toggle selected
            originalPaginationData: null,
            gridModel: {
                //this is used to set a transient whereclause to the grid that should be appended on all subsequent server calls
                fixedWhereClause: null,
                selectedFilter: null
            },
            customSaveFn: null

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
        }

        function currentApplicationName(panelid) {
            return getContext(panelid).currentApplicationName;
        }

        function crudForm(panelid, crudForm) {
            const context = getContext(panelid);
            if (!!crudForm) {
                context.crudForm = crudForm;
            }
            return context.crudForm || {};

        }

        function currentSchema(panelid, schema) {
            const context = getContext(panelid);
            if (schema) {
                context.currentSchema = schema;
            }
            return context.currentSchema;
        }

        function rootDataMap(panelid, datamap) {
            const context = getContext(panelid);
            if (datamap) {
                context.rootDataMap = datamap;
            }
            return context.rootDataMap;
        }

        function originalDatamap(panelid, datamap) {
            const context = getContext(panelid);
            if (datamap) {
                context.originalDatamap = datamap;
            }
            return context.originalDatamap;
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

        const getTabCountQualifier = function (tab) {
            if (!tab.countRelationship) {
                return tab.tabId;
            }
            return tab.countRelationship.endsWith("_") ? tab.countRelationship : tab.countRelationship + "_";
        }

        function getTabRecordCount(tab, panelid) {
            const context = getContext(panelid);
            const qualifier = getTabCountQualifier(tab);
            if (context.tabRecordCount && context.tabRecordCount[qualifier]) {
                return context.tabRecordCount[qualifier];
            }
            return 0;
        }

        function shouldShowRecordCount(tab, panelid) {
            const context = getContext(panelid);
            const qualifier = getTabCountQualifier(tab);
            return context.tabRecordCount && context.tabRecordCount[qualifier];
        }

        function setTabRecordCount(tabId, panelId, count) {
            const context = getContext(panelId);
            if (context.tabRecordCount) {
                context.tabRecordCount[tabId] = count;
            }
        }

        function setDetailDataResolved(panelid) {
            $log.get("crudContextService#setDetailDataResolved", ["dirty", "detail", "datamap"]).debug("marking details as resolved");
            getContext(panelid).detailDataResolved = true;
        }


        function clearDetailDataResolved(panelid, relationshipData) {
            $log.get("crudContextService#setDetailDataResolved", ["dirty", "detail", "datamap"]).debug("cleaning detailresolved flag");
            getContext(panelid).detailDataResolved = false;
        }

        function getDetailDataResolved(panelid) {
            const context = getContext(panelid);
            return context.detailDataResolved && context.associationsResolved && context.compositionLoadComplete;
        }

        //#endregion

        //#region hooks
        function updateCrudContext(schema, rootDataMap, panelid) {
            const log = $log.get("crudContextService#updateCrudContext", ["route"]);
            const context = getContext(panelid);
            schema.properties = schema.properties || {};
            context.currentSchema = schema;
            context.rootDataMap = rootDataMap;
            context.originalDatamap = rootDataMap;
            context.currentApplicationName = schema.applicationName;
            context.gridSelectionModel.selectionMode = "true" === schema.properties["list.selectionmodebydefault"];
            context.gridSelectionModel.selectionBufferIdCollumn = context.gridSelectionModel.selectionBufferIdCollumn || schema.idFieldName;
            schemaCacheService.addSchemaToCache(schema);
            log.debug("crudcontext updated");
        }

    
        function applicationChanged(schema, rootDataMap, panelid) {
            this.clearCrudContext(panelid);
            this.updateCrudContext(schema, rootDataMap, panelid);
            $rootScope.$broadcast(JavascriptEventConstants.AppChanged, schema, rootDataMap, panelid);
        }

        function clearCrudContext(panelid) {
            if (!panelid) {
                _crudContext = angular.copy(_originalContext);
                return _crudContext;
            }
            _crudContexts[panelid] = angular.copy(_originalContext);
            return _crudContext[panelid];
        }

        function afterSave(panelid,datamap) {
            this.clearDirty(panelid);
            getContext(panelid).needsServerRefresh = true;
            this.originalDatamap(panelid, datamap);
            $rootScope.$broadcast(JavascriptEventConstants.CrudSaved);
        }

        function detailLoaded(panelid) {
            this.clearDirty(panelid);
            this.disposeDetail(panelid, false);
            getContext(panelid).needsServerRefresh = false;
            $log.get("crudContextHolderService#detailLoaded", ["navigation", "detail"]).debug("detail loaded");
            $rootScope.$broadcast(JavascriptEventConstants.DetailLoaded);
        }

        function gridLoaded(applicationListResult, panelid) {
            this.disposeDetail(panelid, true);
            this.setActiveTab(null, panelid);
            const context = getContext(panelid);
            context.affectedProfiles = applicationListResult.affectedProfiles;
            context.currentSelectedProfile = applicationListResult.currentSelectedProfile;
            //we need this because the crud_list.js may not be rendered it when this event is dispatched, in that case it should from here when it starts
            contextService.insertIntoContext("grid_refreshdata", { data: applicationListResult, panelid }, true);
        }

        function disposeDetail(panelid, clearTab) {
            const context = getContext(panelid);
            clearDetailDataResolved(panelid);
            context.tabRecordCount = {};
            context._eagerassociationOptions = { "#global": {} };
            _crudContext._lazyAssociationOptions = {};
            context.compositionLoadComplete = false;
            context.associationsResolved = false;
            if (!!clearTab) {
                contextService.setActiveTab(null);
            }
            context.compositionLoadEventQueue = {};
        }

        function compositionsLoaded(result, panelid) {
            $log.get("crudContextService#compositionsLoaded", ["dirty", "detail", "composition"]).debug("marking compositions as resolved");
            const context = getContext(panelid);
            for (let relationship in result) {
                const tab = result[relationship];
                context.tabRecordCount = context.tabRecordCount || {};
                context.tabRecordCount[relationship] = tab.paginationData.totalCount;
            }
            context.compositionLoadComplete = true;
        }

        function clearCompositionsLoaded(panelid) {
            const context = getContext(panelid);
            context.compositionLoadComplete = false;
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
            const log = $log.get("crudcontextHolderService#updateLazyAssociationOption", ["association"]);
            if (!!notIndexed && options != null) {
                const objIdxKey = options.value.toLowerCase();
                const idxedObject = {};
                idxedObject[objIdxKey] = options;
                options = idxedObject;
            }
            var length = "null";
            if (options) {
                length = options.length ? options.length : 1;
            }
            const lazyAssociationOptions = _crudContext._lazyAssociationOptions[associationKey];
            if (lazyAssociationOptions == null) {
                log.debug("creating lazy option(s) to association {0}. size: {1}".format(associationKey, length));
                _crudContext._lazyAssociationOptions[associationKey] = options;
            } else {
                log.debug("appending new option(s) to association {0}. size: {1} ".format(associationKey, length));
                _crudContext._lazyAssociationOptions[associationKey] = angular.extend(lazyAssociationOptions, options);
            }
            //to avoid circular dependency, cannot inject it
            const fieldService = $injector.get("fieldService");
            const displayables = fieldService.getDisplayablesByAssociationKey(_crudContext.currentSchema, associationKey);
            if (displayables && displayables.length === 1 && options) {
                //when we have a reverse relationship, let´s add it to the parentdatamap, to make "life" easier for the outer components, such as the angulartypeahead, 
                //and/or expressions
                const displayable = displayables[0];
                const key = Object.keys(options)[0];
                if (displayable.reverse && key) {
                    //Object.keys(options)[0] --> this would be the key of the association
                    _crudContext.rootDataMap[displayable.target] = key.toLowerCase();
                }
            }


        }

        function blockOrUnblockAssociations(associationKey, blocking) {
            const panelId = this.isShowingModal() ? "#modal" : null;
            getContext(panelId)["_blockedAssociations"][associationKey] = blocking;
        }

        function isAssociationBlocked(associationKey) {
            const panelId = this.isShowingModal() ? "#modal" : null;
            return getContext(panelId)["_blockedAssociations"][associationKey];
        }

        function fetchLazyAssociationOption(associationKey, key, panelid) {
            const associationOptions = _crudContext._lazyAssociationOptions[associationKey];
            if (associationOptions == null) {
                return null;
            }
            const keyToUse = angular.isString(key) ? key.toLowerCase() : key;
            return associationOptions[keyToUse];
        }

        function setDefaultValue(context,path, dmValue) {
            //temporarily setting a value 
            const resultOptions = [];
            //temporarily setting a value 
            resultOptions.push({ value: dmValue, label: "Loading..." });
            setDeep(context._eagerassociationOptions, path, resultOptions);
//            context._eagerassociationOptions[schemaId][entryId][associationKey] = resultOptions;
        }

        /**
         * 
         * @param {} associationKey the association key to bring
         * @param {} contextData an instance of ContextData class (TODO)
         * @param {} panelid the panel we´re handling, used to differentiate on dashboards and modals
         * @param {} dmValue the current value on the datamap, needed in order to instantiate an array with that value upfront preventing bugs on the select component (SWWEB-2607)
         * @returns an Array object with the eager options
         */
        function fetchEagerAssociationOptions(associationKey, contextData, panelid, dmValue) {
            const context = getContext(panelid);
            //if (context.showingModal) {
            //    contextData = { schemaId: "#modal" };
            //}
            let resultOptions;

            if (contextData == null) {
                resultOptions = context._eagerassociationOptions["#global"][associationKey];
                if (dmValue && !resultOptions) {
                    setDefaultValue(context,`#global.${associationKey}`, dmValue);
                }
                return resultOptions;
            }
            const schemaId = contextData.schemaId;
            const entryId = contextData.entryId || "#global";

            context._eagerassociationOptions[schemaId] = context._eagerassociationOptions[schemaId] || {};
            context._eagerassociationOptions[schemaId][entryId] = context._eagerassociationOptions[schemaId][entryId] || {};
            resultOptions = context._eagerassociationOptions[schemaId][entryId][associationKey];
            if (dmValue && !resultOptions) {
                setDefaultValue(context, `${schemaId}.${entryId}.${associationKey}`, dmValue);
            }

            return resultOptions;
        }

        function fetchEagerAssociationOption(associationKey, itemValue) {
            const options = fetchEagerAssociationOptions(associationKey);
            // normalize value to string
            var value = angular.isUndefined(itemValue) || itemValue === null ? itemValue : String(itemValue);
            return !options
                    ? null
                    : options.find(function (a) {
                        return a.value === value;
                    });
        }


        function updateEagerAssociationOptions(associationKey, options, contextData, panelid) {
            if (options == null) {
                //case for dependant associations
                return;
            }
            const context = getContext(panelid);
            if (context.showingModal) {
                contextData = contextData || {};
                contextData.schemaId = "#modal";
            }
            const log = $log.getInstance("crudContext#updateEagerAssociationOptions", ["association"]);
            if (contextData == null) {
                log.info("update eager global list for {0}. Size: {1}".format(associationKey, options.length));
                context._eagerassociationOptions["#global"][associationKey] = options;
                $rootScope.$broadcast(JavascriptEventConstants.Association_EagerOptionUpdated, associationKey, options, contextData);
                return;
            }
            const schemaId = contextData.schemaId;
            const entryId = contextData.entryId || "#global";
            context._eagerassociationOptions[schemaId] = context._eagerassociationOptions[schemaId] || {};
            context._eagerassociationOptions[schemaId][entryId] = context._eagerassociationOptions[schemaId][entryId] || {};

            context._eagerassociationOptions[schemaId][entryId][associationKey] = options;

            log.info("update eager list for {0}. Size: {1}".format(associationKey, options.length));


            $rootScope.$broadcast(JavascriptEventConstants.Association_EagerOptionUpdated, associationKey, options, contextData, panelid);


        }

        function markAssociationsResolved(panelid) {
            $log.get("crudContextService#markAssociationsResolved", ["dirty", "detail", "association"]).debug("marking associations as resolved");
            getContext(panelid).associationsResolved = true;
            $rootScope.$broadcast(JavascriptEventConstants.AssociationResolved, panelid);
            contextService.insertIntoContext("associationsresolved", true, true);
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

        function modalLoaded(datamap, schema) {
            _crudContext.showingModal = true;
            rootDataMap("#modal", datamap);
            currentSchema("#modal", schema);
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

        function registerPrimaryCommand(command) {
            getContext("#modal").primaryCommand = command;
        };

        function getPrimaryCommand() {
            return getContext("#modal").primaryCommand;
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
            const context = getContext(panelid);
            context.gridSelectionModel.showOnlySelected = !context.gridSelectionModel.showOnlySelected;
            return context.gridSelectionModel.showOnlySelected;
        }


        function getSelectionModel(panelid) {
            const context = getContext(panelid);
            return context.gridSelectionModel;
        }

        function toggleSelectionMode(panelid) {
            const context = getContext(panelid);
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

        function setFixedWhereClause(panelId, fixedWhereClause) {
            const context = getContext(panelId);
            context.gridModel.fixedWhereClause = fixedWhereClause;
        }

        function getFixedWhereClause(panelId) {
            const context = getContext(panelId);
            return context.gridModel.fixedWhereClause;
        }

        function setSelectedFilter(filter, panelId) {
            const context = getContext(panelId);
            context.gridModel.selectedFilter = filter;
        }

        function getSelectedFilter(panelId) {
            const context = getContext(panelId);
            return context.gridModel.selectedFilter;
        }

        //#endregion

        //#region commandsServices

        function getCommandsModel(panelid) {
            return getContext(panelid).commandsModel;
        }

        function getToggleCommand(commandId, panelid) {
            return getCommandsModel(panelid).toggleCommands[commandId];
        }

        function addToggleCommand(command, panelid) {
            return getCommandsModel(panelid).toggleCommands[command.id] = command;
        }

        //#endregion


        //#region navigationServices

        function isList(panelid) {
            return getContext(panelid).isList;
        }

        function isDetail(commandId, panelid) {
            return getContext(panelid).isDetail;
        }

        function setList(list) {
            getContext(panelid).isList = true;
            getContext(panelid).isDetail = false;
        }

        function setDetail(list) {
            getContext(panelid).isList = false;
            getContext(panelid).isDetail = true;
        }

        function compositionQueue(panelid) {
            return getContext(panelid).compositionLoadEventQueue;
        }



        //#endregion

        //#region Service Instance
        const generalServices = {
            getAffectedProfiles,
            getActiveTab,
            setActiveTab,
            getTabRecordCount,
            setTabRecordCount,
            shouldShowRecordCount,
            getCurrentSelectedProfile,
            setCurrentSelectedProfile,
            currentSchema,
            currentApplicationName,
            updateCrudContext,
            applicationChanged,
            clearCrudContext,
            needsServerRefresh,
            rootDataMap,
            originalDatamap,
            setDetailDataResolved,
            getDetailDataResolved,
            clearDetailDataResolved,
            getContext,
            crudForm
        };
        const associationServices = {
            updateLazyAssociationOption,
            fetchLazyAssociationOption,
            updateEagerAssociationOptions,
            fetchEagerAssociationOptions,
            fetchEagerAssociationOption,
            associationsResolved,
            markAssociationsResolved,
            blockOrUnblockAssociations,
            isAssociationBlocked
        };
        const hookServices = {
            afterSave,
            detailLoaded,
            disposeDetail,
            gridLoaded,
            compositionsLoaded,
            clearCompositionsLoaded
        };
        const modalService = {
            disposeModal,
            getSaveFn,
            getPrimaryCommand,
            isShowingModal,
            modalLoaded,
            registerSaveFn,
            registerPrimaryCommand
        };
        const gridServices = {
            setFixedWhereClause,
            getFixedWhereClause,
            setSelectedFilter,
            getSelectedFilter
        };
        const selectionService = {
            addSelectionToBuffer,
            clearSelectionBuffer,
            getOriginalPaginationData,
            getSelectionModel,
            removeSelectionFromBuffer,
            setOriginalPaginationData,
            toggleSelectionMode,
            toggleShowOnlySelected
        };
        const commandsServices = {
            getCommandsModel,
            getToggleCommand,
            addToggleCommand
        };
        const detailServices = {
            setDirty,
            getDirty,
            clearDirty,
            compositionQueue
        };
        const navigationServices = {
            isList,
            isDetail,
            setList,
            setDetail,
        };
        return angular.extend({}, generalServices, hookServices, associationServices, modalService, selectionService, gridServices, commandsServices, detailServices, navigationServices);


        //#endregion
    }


    angular.module("sw_layout").service("crudContextHolderService", ["$rootScope", "$log", "$injector", "$timeout", "contextService", "schemaCacheService", crudContextHolderService]);



})(angular);
