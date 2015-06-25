var constants = constants || {};


mobileServices.factory('crudContextService', function ($q, $log, swdbDAO,
    metadataModelService, offlineSchemaService, offlineCompositionService,
    offlineSaveService, schemaService, contextService, routeService, tabsService,
    crudFilterContextService) {
    'use strict';

    var internalListContext = {
        lastPageLoaded: 1
    }

    var crudContext = {
        currentApplicationName: null,
        currentApplication: null,
        currentTitle: null,

        currentListSchema: null,
        itemlist: null,
        filteredList: null,

        originalDetailItemDatamap: null,
        currentDetailItem: null,
        currentDetailSchema: null,

        //composition
        composition: {
            currentTab: null,
            currentListSchema: null,
            itemlist: null,

            currentDetailItem: null,
            originalDetailItemDatamap: null,
            currentDetailSchema: null,


        },


        previousItem: null,
        nextItem: null,

    }




    function loadDetailSchema() {
        var detailSchemaId = "detail";
        var overridenSchema = schemaService.getProperty(crudContext.currentListSchema, "list.click.schema");
        if (overridenSchema) {
            detailSchemaId = overridenSchema;
        }
        return offlineSchemaService.locateSchema(crudContext.currentApplication, detailSchemaId);
    }

    function setPreviousAndNextItems(item) {
        if (!item) {
            return;
        }
        var itemlist = crudContext.itemlist;
        var idx = itemlist.indexOf(item);
        if (idx == 0) {
            crudContext.previousItem = null;
        } else {
            crudContext.previousItem = itemlist[idx - 1];
        }
        if (idx >= itemlist.length - 2) {
            crudContext.nextItem = null;
        } else {
            crudContext.nextItem = itemlist[idx + 1];
        }
    }

    return {

        restoreState: function () {
            if (!isRippleEmulator()) {
                return; //this is used for F5 (refresh) upon development mode, so that we can return to the page we were before quickier
            }
            var savedCrudContext = contextService.getFromContext("crudcontext");
            if (savedCrudContext) {
                crudContext = JSON.parse(savedCrudContext);
                if (crudContext.itemlist) {
                    crudContext.itemlist = [];
                    if (!crudContext.currentDetailItem) {
                        this.refreshGrid();
                    }
                }
                if (crudContext.originalDetailItemDatamap) {
                    // the persistence entries do not get serialized correctly
                    crudContext.originalDetailItemDatamap = angular.copy(crudContext.originalDetailItemDatamap);
                    crudContext.currentDetailItem.datamap = crudContext.originalDetailItemDatamap;
                    setPreviousAndNextItems(savedCrudContext.currentDetailItem);
                }
                
            }
            $log.get("crudContextService#factory").debug("restoring state of crudcontext");
            return savedCrudContext;
        },

        isList: function () {
            return crudContext.currentDetailItem == null;
        },

        getFilteredList: function () {
            return crudContext.filteredList;
        },


        currentTitle: function () {
            var tabTitle = this.tabTitle();
            if (tabTitle != null) {
                return crudContext.currentTitle + " / " + tabTitle;
            }

            return crudContext.currentTitle;
        },

        currentApplicationName: function () {
            return crudContext.currentApplicationName;
        },

        currentListSchema: function () {
            return crudContext.currentListSchema;
        },

        currentDetailSchema: function () {
            return crudContext.currentDetailSchema;
        },

        currentDetailItem: function () {
            return crudContext.currentDetailItem;
        },

        currentDetailItemDataMap: function () {
            if (crudContext.composition.currentDetailItem) {
                return crudContext.composition.currentDetailItem;
            }

            return crudContext.currentDetailItem.datamap;
        },

        itemlist: function () {
            return crudContext.itemlist;
        },





        mainDisplayables: function () {
            return schemaService.nonTabFields(crudContext.currentDetailSchema);
        },


        /*******************************************COMPOSITIONS******************************************************/

        currentCompositionsToShow: function () {
            var detailSchema = crudContext.currentDetailSchema;
            var allDisplayables = tabsService.tabsDisplayables(detailSchema);
            return allDisplayables;
        },

        leavingDetail: function () {
            crudContext.composition = {};
            crudContext.currentDetailItem = null;
        },

        isOnMainTab: function () {
            return crudContext.composition.currentTab == null;
        },

        tabTitle: function () {
            if (this.isOnMainTab()) {
                return null;
            }
            return crudContext.composition.currentTab.label;
        },

        resetTab: function () {
            crudContext.composition.currentTab = null;
        },

        leavingCompositionDetail: function () {
            crudContext.composition.currentDetailItem = null;
        },


        loadTab: function (tab) {
            if (tab == null) {
                //let´s return to the main tab
                this.resetTab();
                return routeService.go("main.cruddetail.maininput");
            }

            if (tab.type != "ApplicationCompositionDefinition") {
                //tabs do not need to load from the database since the data is already contained on the main datamap
                crudContext.composition.currentTab = tab;
                return routeService.go("main.cruddetail.compositionlist");
            }

            return offlineCompositionService.loadComposition(crudContext.currentDetailItem, tab).then(function (compositionItems) {
                crudContext.composition.currentTab = tab;
                crudContext.composition.itemlist = compositionItems;
                crudContext.composition.currentListSchema = tab.schema.schemas.list;
                crudContext.composition.currentDetailSchema = tab.schema.schemas.detail;
                crudContext.composition.currentDetailItem = null;
                contextService.insertIntoContext("crudcontext", crudContext);
                return routeService.go("main.cruddetail.compositionlist");
            });
        },

        loadCompositionDetail: function (item) {
            //for compositions item will be the datamap itself

            crudContext.composition.currentDetailItem = item;
            crudContext.composition.originalDetailItemDatamap = angular.copy(item);
            contextService.insertIntoContext("crudcontext", crudContext);
            return routeService.go("main.cruddetail.compositiondetail");
        },

        createNewCompositionItem: function () {
            crudContext.composition.currentDetailItem = {};
            offlineSchemaService.fillDefaultValues(crudContext.composition.currentDetailSchema, crudContext.composition.currentDetailItem);
            crudContext.composition.originalDetailItemDatamap = {
                //to make this new item always dirty!!!
                "_newitem#$": true
            };
            contextService.insertIntoContext("crudcontext", crudContext);
            return routeService.go("main.cruddetail.compositiondetail");
        },




        compositionList: function () {
            return crudContext.composition.itemlist;
        },

        getCompositionListSchema: function () {
            return crudContext.composition.currentListSchema;
        },

        getCompositionDetailSchema: function () {
            return crudContext.composition.currentDetailSchema;
        },

        getCompositionDetailItem: function () {
            return crudContext.composition.currentDetailItem;
        },

        /**********************************************************************************************************************/


        /**************************************************************************SAVE FNS********************************************************************************************/

        hasDirtyChanges: function () {
            if (crudContext.composition.currentDetailItem) {
                return crudContext.composition.currentDetailItem && (!angular.equals(crudContext.composition.originalDetailItemDatamap, crudContext.composition.currentDetailItem));
            }

            return crudContext.currentDetailItem && (!angular.equals(crudContext.originalDetailItemDatamap, crudContext.currentDetailItem.datamap));
        },

        cancelChanges: function () {
            if (crudContext.composition.currentDetailItem) {
                crudContext.composition.currentDetailItem = angular.copy(crudContext.composition.originalDetailItemDatamap);
                return routeService.go("main.cruddetail.compositionlist");
            }
            crudContext.currentDetailItem.datamap = angular.copy(crudContext.originalDetailItemDatamap);
        },

        saveChanges: function () {

            var datamap = crudContext.currentDetailItem.datamap;
            var that = this;
            if (crudContext.composition && crudContext.composition.currentDetailItem) {
                var compositionItem = crudContext.composition.currentDetailItem;
                return offlineSaveService.addAndSaveComposition(crudContext.currentApplicationName, datamap, compositionItem, crudContext.composition.currentTab).then(function () {
                    crudContext.composition.originalDetailItemDatamap = crudContext.composition.currentDetailItem;
                    that.loadTab(crudContext.composition.currentTab);
                });
            }

            return offlineSaveService.saveItem(crudContext.currentApplicationName, datamap).then(function () {
                crudContext.originalDetailItemDatamap = angular.copy(datamap);
                contextService.insertIntoContext("crudcontext", crudContext);
            });
        },

        /**************************************************************************END SAVE FNS********************************************************************************************/

        /**************************************************************************GRID FNS********************************************************************************************/


        filterList: function (text) {

            internalListContext.searchQuery = text;
            if (text == null) {
                return;
            }
            crudContext.filteredList = crudContext.filteredList || [];
            internalListContext.pageNumber = 1;
            internalListContext.lastPageLoaded = 0;
            this.loadMorePromise();
        },

        refreshGrid: function () {
            internalListContext.searchQuery = null;
            crudContext.itemlist = [];
            internalListContext.lastPageLoaded = 0;
            internalListContext.pageNumber = 1;
            this.loadMorePromise().then(function () {
                routeService.go("main.crudlist");
                contextService.insertIntoContext("crudcontext", crudContext);
            });
        },

        loadMorePromise: function () {
            var baseQuery = "application = '{0}'".format(crudContext.currentApplicationName);
            var filteredMode = false;
            if (!nullOrEmpty(internalListContext.searchQuery)) {
                filteredMode = true;
                baseQuery += ' and datamap like \'%:"{0}%\''.format(internalListContext.searchQuery);
            }
            if (!crudFilterContextService.showPending()) {
                baseQuery += ' and pending = 0 ';
            }
            if (!crudFilterContextService.showDirty()) {
                baseQuery += ' and isDirty = 0 ';
            }

            return swdbDAO.findByQuery("DataEntry", baseQuery, { pagesize: 10, pagenumber: internalListContext.pageNumber }).then(function (results) {
                internalListContext.lastPageLoaded = internalListContext.lastPageLoaded + 1;
                if (filteredMode) {
                    crudContext.filteredList = [];
                }
                var listToPush = filteredMode ? crudContext.filteredList : crudContext.itemlist;
                for (var i = 0; i < results.length; i++) {
                    listToPush.push(results[i]);
                }
                return $q.when(results);
            });
        },


        loadApplicationGrid: function (applicationName, applicationTitle, schemaId) {
            crudContext.currentTitle = applicationTitle;
            var application = metadataModelService.getApplicationByName(applicationName);

            crudContext.currentApplicationName = applicationName;
            crudContext.currentApplication = application;
            crudContext.currentListSchema = offlineSchemaService.locateSchema(application, schemaId);


            crudContext.currentDetailSchema = loadDetailSchema();
            crudContext.currentDetailItem = null;
            crudContext.composition = {};
            this.refreshGrid();
        },



        /**************************************************************************END GRID FNS********************************************************************************************/

        navigatePrevious: function () {
            if (!crudContext.previousItem) {
                routeService.go("main.crudlist");
            } else {
                this.loadDetail(crudContext.previousItem);
            }

        },

        navigateNext: function () {
            var outer = this;
            if (!crudContext.nextItem) {
                return this.loadMorePromise().then(function (results) {
                    if (!results) {
                        //end has reached;
                        return;
                    }
                    setPreviousAndNextItems(crudContext.currentDetailItem);
                    outer.loadDetail(crudContext.nextItem);
                });
            }
            this.loadDetail(crudContext.nextItem);
            return $q.when();
        },

        createDetail: function () {
            crudContext.currentDetailItem = {};
            offlineSchemaService.fillDefaultValues(this.currentDetailSchema(), crudContext.currentDetailItem);
            crudContext.originalDetailItemDatamap = {
                //to make this new item always dirty!!!
                "_newitem#$": true
            };
            return routeService.go("main.cruddetail.maininput");
        },

        loadDetail: function (item) {
            /// <summary>
            ///  Loads a detail represented by the parameter item.
            /// </summary>
            /// <param name="item"></param>
            /// <returns type=""></returns>
            if (!crudContext.currentDetailSchema) {
                crudContext.currentDetailSchema = loadDetailSchema();
            }
            crudContext.currentDetailItem = item;
            crudContext.originalDetailItemDatamap = angular.copy(crudContext.currentDetailItem.datamap);
            setPreviousAndNextItems(item);
            contextService.insertIntoContext("crudcontext", crudContext);
            return routeService.go("main.cruddetail.maininput");
        }


    }

});