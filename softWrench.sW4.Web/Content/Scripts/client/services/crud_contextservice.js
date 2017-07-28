
(function (angular) {
    "use strict";

    class crudContextHolderService{

        constructor($rootScope, $log, $injector,contextService, schemaCacheService) {
            this.$rootScope = $rootScope;
            this.$log = $log;
            this.$injector = $injector;
            this.contextService = contextService;
            this.schemaCacheService = schemaCacheService;

            this._originalSortModel = {
                sortColumns: [],
                multiSortVisible: false
            }

            this._originalContext = {
                currentSchema: null,
                rootDataMap: null,
                originalDatamap: null,
                isList: null,
                isDetail: null,
                crudForm: null,

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
                customSaveFn: null,
                sortModel: angular.copy(this._originalSortModel)
        };

            //#region private variables



            //TODO: continue implementing this methods, removing crud_context object references from the contextService
            // ReSharper disable once InconsistentNaming
     

            this._crudContext = angular.copy(this._originalContext);

            this._crudContexts = {};

        }

        

        /**
         * Returns (or create an returns) a context for the given paneilId.
         * Isolates the crud context of distinct panels.
         * All API methods are based on panelId and only affects the specific context.
         * 
         * @param {} panelid 
         * @returns {} 
         */
         getContext (panelid) {
            if (!panelid) {
                return this._crudContext;
            }
            var context = this._crudContexts[panelid];
            if (!context) {
                this._crudContexts[panelid] = angular.copy(this._originalContext);
                context = this._crudContexts[panelid];
            }
            return context;
        }

        //#endregion

        //#region Public methods

        //#region simple getters/setters

        getActiveTab() {
            return this.contextService.getActiveTab();
        }

        setActiveTab(tabId) {
            this.contextService.setActiveTab(tabId);
        }

        currentApplicationName(panelid) {
            return this.getContext(panelid).currentApplicationName;
        }

        crudForm(panelid, crudForm) {
            const context = this.getContext(panelid);
            if (!!crudForm) {
                context.crudForm = crudForm;
            }
            return context.crudForm || {};
        }

        currentSchema(panelid, schema) {
            const context = this.getContext(panelid);
            if (schema) {
                context.currentSchema = schema;
            }
            return context.currentSchema;
        }

        rootDataMap(panelid, datamap) {
            const context = this.getContext(panelid);
            if (datamap) {
                context.rootDataMap = datamap;
            }
            return context.rootDataMap;
        }

        rootId() {
            const dm = this.rootDataMap();
            const schema = this.currentSchema();
            return dm[schema.idFieldName];
        }

        originalDatamap(panelid, datamap) {
            const context = this.getContext(panelid);
            if (datamap) {
                context.originalDatamap = angular.copy(datamap);
            }
            return context.originalDatamap;
        }

        getAffectedProfiles(panelid) {
            return this.getContext(panelid).affectedProfiles || [];
        }

        getCurrentSelectedProfile(panelid) {
            return this.getContext(panelid).currentSelectedProfile;
        }

        getConstrainedProfiles(panelid) {
            return this.getContext(panelid).constrainedProfiles;
        }

        setConstrainedProfiles(constrainedProfiles,panelid) {
            return this.getContext(panelid).constrainedProfiles = constrainedProfiles;
        }

        setCurrentSelectedProfile(currentProfile, panelid) {
            return this.getContext(panelid).currentSelectedProfile = currentProfile;
        }

        setDirty(panelid) {
           this.getContext(panelid).isDirty = true;
        }

        getDirty(panelid) {
            return this.getContext(panelid).isDirty;
        }

        clearDirty(panelid) {
            this.getContext(panelid).isDirty = false;
        }

        needsServerRefresh(panelid) {
            return this.getContext(panelid).needsServerRefresh;
        }

        getTabCountQualifier (tab) {
            if (!tab.countRelationship) {
                return tab.tabId;
            }
            return tab.countRelationship.endsWith("_") ? tab.countRelationship : tab.countRelationship + "_";
        }

        getTabRecordCount(tab, panelid) {
            const context = this.getContext(panelid);
            const qualifier = this.getTabCountQualifier(tab);
            if (context.tabRecordCount && context.tabRecordCount[qualifier]) {
                return context.tabRecordCount[qualifier];
            }
            return 0;
        }

        shouldShowRecordCount(tab, panelid) {
            const context = this.getContext(panelid);
            const qualifier = this.getTabCountQualifier(tab);
            return context.tabRecordCount && context.tabRecordCount[qualifier];
        }

        setTabRecordCount(tabId, panelId, count) {
            const context = this.getContext(panelId);
            if (context.tabRecordCount) {
                context.tabRecordCount[tabId] = count;
            }
        }

        setDetailDataResolved(panelid) {
            this.$log.get("crudContextService#setDetailDataResolved", ["dirty", "detail", "datamap"]).debug("marking details as resolved (even if temporarily)");
            this.getContext(panelid).detailDataResolved = true;
        }


        clearDetailDataResolved(panelid, relationshipData) {
            this.$log.get("crudContextService#setDetailDataResolved", ["dirty", "detail", "datamap"]).debug("cleaning detailresolved flag (even if temporarily)");
            this.getContext(panelid).detailDataResolved = false;
        }

        getDetailDataResolved(panelid) {
            const context = this.getContext(panelid);
            return context.detailDataResolved && context.associationsResolved && context.compositionLoadComplete;
        }

        getSortModel(panelid) {
            return this.getContext(panelid).sortModel;
        }

        clearSortModel(panelid) {
            return this.getContext(panelid).sortModel = {
                sortColumns: [],
                multiSortVisible: false
            };
        }

        //#endregion

        //#region hooks
        updateCrudContext(schema, rootDataMap, panelid) {
            const log = this.$log.get("crudContextService#updateCrudContext", ["route"]);
            const context = this.getContext(panelid);
            schema.properties = schema.properties || {};
            context.currentSchema = schema;
            context.rootDataMap = rootDataMap;
            context.originalDatamap = rootDataMap;
            context.currentApplicationName = schema.applicationName;
            context.gridSelectionModel.selectionMode = "true" === schema.properties["list.selectionmodebydefault"];
            context.gridSelectionModel.selectionBufferIdCollumn = context.gridSelectionModel.selectionBufferIdCollumn || schema.idFieldName;
            this.schemaCacheService.addSchemaToCache(schema);
            log.debug("crudcontext updated");
        }

    
        applicationChanged(schema, rootDataMap, panelid) {
            this.clearCrudContext(panelid);
            this.updateCrudContext(schema, rootDataMap, panelid);
            this.$rootScope.$broadcast(JavascriptEventConstants.AppChanged, schema, rootDataMap, panelid);
        }

        clearCrudContext(panelid) {
            if (!panelid) {
                this._crudContext = angular.copy(this._originalContext);
                return this._crudContext;
            }
            this._crudContexts[panelid] = angular.copy(this._originalContext);
            return this._crudContext[panelid];
        }

        afterSave(panelid,datamap) {
            this.clearDirty(panelid);
            this.getContext(panelid).needsServerRefresh = true;
            this.originalDatamap(panelid, datamap);
            this.$rootScope.$broadcast(JavascriptEventConstants.CrudSaved);
        }

        detailLoaded(panelid) {
            this.clearDirty(panelid);
            this.disposeDetail(panelid, false);
            this.getContext(panelid).needsServerRefresh = false;
            this.$log.get("crudContextHolderService#detailLoaded", ["navigation", "detail"]).debug("detail loaded");
            this.$rootScope.$broadcast(JavascriptEventConstants.DetailLoaded);
        }

        gridLoaded(applicationListResult, panelid) {
            this.disposeDetail(panelid, true);
            this.setActiveTab(null, panelid);
            const context = this.getContext(panelid);
            context.affectedProfiles = applicationListResult.affectedProfiles;
            context.currentSelectedProfile = applicationListResult.currentSelectedProfile;
            //we need this because the crud_list.js may not be rendered it when this event is dispatched, in that case it should from here when it starts
            this.contextService.insertIntoContext("grid_refreshdata", { data: applicationListResult, panelid }, true);
        }

        lookupDataLoaded(lookupResult) {
            const context = this.getContext("#modal");
            context.affectedProfiles = lookupResult.affectedProfiles;
            context.currentSelectedProfile = lookupResult.currentSelectedProfile;
        }

        disposeDetail(panelid, clearTab) {
            const context = this.getContext(panelid);
            this.clearDetailDataResolved(panelid);
            context.tabRecordCount = {};
            context._eagerassociationOptions = { "#global": {} };
            this._crudContext._lazyAssociationOptions = {};
            context.compositionLoadComplete = false;
            context.associationsResolved = false;
            if (!!clearTab) {
                this.contextService.setActiveTab(null);
            }
            context.compositionLoadEventQueue = {};
        }

        compositionsLoaded(result, panelid) {
            this.$log.get("crudContextService#compositionsLoaded", ["dirty", "detail", "composition"]).debug("marking compositions as resolved");
            const context = this.getContext(panelid);
            for (let relationship in result) {
                if (result.hasOwnProperty(relationship)) {
                    const tab = result[relationship];
                    context.tabRecordCount = context.tabRecordCount || {};
                    context.tabRecordCount[relationship] = tab.paginationData.totalCount;
                }
            }
            context.compositionLoadComplete = true;
        }

        clearCompositionsLoaded(panelid) {
            const context = this.getContext(panelid);
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
        updateLazyAssociationOption(associationKey, options, notIndexed, panelid) {
            const log = this.$log.get("crudcontextHolderService#updateLazyAssociationOption", ["association"]);
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
            const lazyAssociationOptions = this._crudContext._lazyAssociationOptions[associationKey];
            if (lazyAssociationOptions == null) {
                log.debug("creating lazy option(s) to association {0}. size: {1}".format(associationKey, length));
                this._crudContext._lazyAssociationOptions[associationKey] = options;
            } else {
                log.debug("appending new option(s) to association {0}. size: {1} ".format(associationKey, length));
                this._crudContext._lazyAssociationOptions[associationKey] = angular.extend(lazyAssociationOptions, options);
            }
            //to avoid circular dependency, cannot inject it
            const fieldService = this.$injector.get("fieldService");
            const displayables = fieldService.getDisplayablesByAssociationKey(this._crudContext.currentSchema, associationKey);
            if (displayables && displayables.length === 1 && options) {
                //when we have a reverse relationship, let´s add it to the parentdatamap, to make "life" easier for the outer components, such as the angulartypeahead, 
                //and/or expressions
                const displayable = displayables[0];
                const key = Object.keys(options)[0];
                if (displayable.reverse && key) {
                    //Object.keys(options)[0] --> this would be the key of the association
                    this._crudContext.rootDataMap[displayable.target] = key.toLowerCase();
                }
            }


        }

        blockOrUnblockAssociations(associationKey, blocking) {
            const panelId = this.isShowingModal() ? "#modal" : null;
            this.getContext(panelId)["_blockedAssociations"][associationKey] = blocking;
        }

        isAssociationBlocked(associationKey) {
            const panelId = this.isShowingModal() ? "#modal" : null;
            return this.getContext(panelId)["_blockedAssociations"][associationKey];
        }

        fetchLazyAssociationOption(associationKey, key, panelid) {
            const associationOptions = this._crudContext._lazyAssociationOptions[associationKey];
            if (associationOptions == null) {
                return null;
            }
            const keyToUse = angular.isString(key) ? key.toLowerCase() : key;
            return associationOptions[keyToUse];
        }

        setDefaultValue(context,path, dmValue) {
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
        fetchEagerAssociationOptions(associationKey, contextData, panelid, dmValue) {
            const context = this.getContext(panelid);
            //if (context.showingModal) {
            //    contextData = { schemaId: "#modal" };
            //}
            let resultOptions;

            if (contextData == null) {
                resultOptions = context._eagerassociationOptions["#global"][associationKey];
                if (dmValue && !resultOptions) {
                    this.setDefaultValue(context,`#global.${associationKey}`, dmValue);
                }
                return resultOptions;
            }
            const schemaId = contextData.schemaId;
            const entryId = contextData.entryId || "#global";

            context._eagerassociationOptions[schemaId] = context._eagerassociationOptions[schemaId] || {};
            context._eagerassociationOptions[schemaId][entryId] = context._eagerassociationOptions[schemaId][entryId] || {};
            resultOptions = context._eagerassociationOptions[schemaId][entryId][associationKey];
            if (dmValue && !resultOptions) {
                this.setDefaultValue(context, `${schemaId}.${entryId}.${associationKey}`, dmValue);
            }

            return resultOptions;
        }

        fetchEagerAssociationOption(associationKey, itemValue) {
            const options = this.fetchEagerAssociationOptions(associationKey);
            // normalize value to string
            var value = angular.isUndefined(itemValue) || itemValue === null ? itemValue : String(itemValue);
            return !options
                    ? null
                    : options.find(function (a) {
                        return a.value === value;
                    });
        }


        updateEagerAssociationOptions(associationKey, options, contextData, panelid) {
            if (options == null) {
                //case for dependant associations
                return;
            }
            let context = this.getContext(panelid);
            if (context.showingModal && !panelid) {
                contextData = contextData || {};
                contextData.schemaId = "#modal";
                context = this.getContext("#modal");
            }
            const log = this.$log.getInstance("crudContext#updateEagerAssociationOptions", ["association"]);
            if (contextData == null) {
                log.info("update eager global list for {0}. Size: {1}".format(associationKey, options.length));
                context._eagerassociationOptions["#global"][associationKey] = options;
                this.$rootScope.$broadcast(JavascriptEventConstants.Association_EagerOptionUpdated, associationKey, options, contextData);
                return;
            }
            const schemaId = contextData.schemaId;
            const entryId = contextData.entryId || "#global";
            context._eagerassociationOptions[schemaId] = context._eagerassociationOptions[schemaId] || {};
            context._eagerassociationOptions[schemaId][entryId] = context._eagerassociationOptions[schemaId][entryId] || {};

            context._eagerassociationOptions[schemaId][entryId][associationKey] = options;

            log.info("update eager list for {0}. Size: {1}".format(associationKey, options.length));


            this.$rootScope.$broadcast(JavascriptEventConstants.Association_EagerOptionUpdated, associationKey, options, contextData, panelid);


        }

        markAssociationsResolved(panelid) {
            this.$log.get("crudContextService#markAssociationsResolved", ["dirty", "detail", "association"]).debug("marking associations as resolved");
            this.getContext(panelid).associationsResolved = true;
            this.$rootScope.$broadcast(JavascriptEventConstants.AssociationResolved, panelid);
            this.contextService.insertIntoContext("associationsresolved", true, true);
        }

        associationsResolved(panelid) {
            return this.getContext(panelid).associationsResolved;
        }


        //#endregion

        //#region modal

        disposeModal() {
            this._crudContext.showingModal = false;
            this._crudContext._eagerassociationOptions["#modal"] = { "#global": {} };
            this.clearCrudContext("#modal");
        }

        modalLoaded(datamap, schema) {
            this._crudContext.showingModal = true;
            this.rootDataMap("#modal", datamap);
            this.currentSchema("#modal", schema);
        }

        isShowingModal() {
            return this._crudContext.showingModal;
        }

        registerSaveFn(saveFn) {
            this.getContext("#modal").customSaveFn = saveFn;
        }

         getSaveFn() {
             return this.getContext("#modal").customSaveFn;
        }

         registerPrimaryCommand(command) {
             this.getContext("#modal").primaryCommand = command;
        }

        getPrimaryCommand() {
            return this.getContext("#modal").primaryCommand;
        }


        //#endregion

        //#region selectionService



        addSelectionToBuffer(rowId, row, panelid) {
            this.getContext(panelid).gridSelectionModel.selectionBuffer[rowId] = row;
        }

        removeSelectionFromBuffer(rowId, panelid) {
            delete this.getContext(panelid).gridSelectionModel.selectionBuffer[rowId];
        }

        clearSelectionBuffer(panelid) {
            this.getContext(panelid).gridSelectionModel.selectionBuffer = {};
        }

        toggleShowOnlySelected(panelid) {
            const context = this.getContext(panelid);
            context.gridSelectionModel.showOnlySelected = !context.gridSelectionModel.showOnlySelected;
            return context.gridSelectionModel.showOnlySelected;
        }


        getSelectionModel(panelid) {
            const context = this.getContext(panelid);
            return context.gridSelectionModel;
        }

        toggleSelectionMode(panelid) {
            const context = this.getContext(panelid);
            context.gridSelectionModel.selectionMode = !context.gridSelectionModel.selectionMode;
            return context.gridSelectionModel.selectionMode;
        }

        getOriginalPaginationData(panelid) {
            return this.getContext(panelid).originalPaginationData;
        }

        setOriginalPaginationData(paginationData, panelid) {
            this.getContext(panelid).originalPaginationData = angular.copy(paginationData);
        }
        //#endregion

        //#region gridServices

        setFixedWhereClause(panelId, fixedWhereClause) {
            const context = this.getContext(panelId);
            context.gridModel.fixedWhereClause = fixedWhereClause;
        }

        getFixedWhereClause(panelId) {
            const context = this.getContext(panelId);
            return context.gridModel.fixedWhereClause;
        }

        setSelectedFilter(filter, panelId) {
            const context = this.getContext(panelId);
            context.gridModel.selectedFilter = filter;
        }

        getSelectedFilter(panelId) {
            const context = this.getContext(panelId);
            return context.gridModel.selectedFilter;
        }

        //#endregion

        //#region commandsServices

        getCommandsModel(panelid) {
            return this.getContext(panelid).commandsModel;
        }

        getToggleCommand(commandId, panelid) {
            return this.getCommandsModel(panelid).toggleCommands[commandId];
        }

        addToggleCommand(command, panelid) {
            return this.getCommandsModel(panelid).toggleCommands[command.id] = command;
        }

        //#endregion


        //#region navigationServices

        usebackHistoryNavigation(useBackHistoryNavigation) {
            if (useBackHistoryNavigation !== undefined) {
                this.getContext().useBackHistoryNavigation = useBackHistoryNavigation;
                return useBackHistoryNavigation;
            }
            return this.getContext().useBackHistoryNavigation;
        }

        isList(panelid) {
            return this.getContext(panelid).isList;
        }

        isDetail(commandId, panelid) {
            return this.getContext(panelid).isDetail;
        }

        setList(list) {
            this.getContext(panelid).isList = true;
            this.getContext(panelid).isDetail = false;
        }

        setDetail(list) {
            this.getContext(panelid).isList = false;
            this.getContext(panelid).isDetail = true;
        }

        compositionQueue(panelid) {
            return this.getContext(panelid).compositionLoadEventQueue;
        }


    }

    crudContextHolderService.$inject = ["$rootScope", "$log", "$injector", "contextService", "schemaCacheService"];

    angular.module("sw_layout").service("crudContextHolderService", crudContextHolderService);



})(angular);
