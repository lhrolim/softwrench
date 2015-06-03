var constants = constants || {};


mobileServices.factory('crudContextService', function ($q, $log, swdbDAO, metadataModelService, offlineSchemaService, offlineCompositionService, schemaService, contextService, routeService, tabsService) {
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


        currentDetailItem: null,
        currentDetailSchema: null,

        //composition
        composition: {
            currentTab: null,
            currentListSchema: null,
            itemlist: null,

            currentDetailItem: null,
            currentDetailSchema: null,


        },


        previousItem: null,
        nextItem: null,

    }

    if (isRippleEmulator()) {
        //this is used for F5 (refresh) upon development mode, so that we can return to the page we were before quickier
        var savedCrudContext = contextService.getFromContext("crudcontext");
        if (savedCrudContext) {
            crudContext = JSON.parse(savedCrudContext);
            setPreviousAndNextItems(savedCrudContext.currentDetailItem);
        }
        $log.get("crudContextService#factory").debug("restoring state of crudcontext");
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

        currentTitle: function () {
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

        leavingDetail : function() {
            crudContext.composition = {};
        },

        isOnMainTab: function () {
            return crudContext.composition.currentTab == null;
        },

        tabTitle: function() {
            if (this.isOnMainTab()) {
                return null;
            }
            return crudContext.composition.currentTab.label;
        },

        loadTab: function (tab) {
            if (tab == null) {
                //let´s return to the main tab
                crudContext.composition.currentTab = null;
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
                contextService.insertIntoContext("crudcontext", crudContext);
                return routeService.go("main.cruddetail.compositionlist");
            });
        },

        loadCompositionDetail:function(item) {
            crudContext.composition.currentDetailItem = item;
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

        loadMorePromise: function () {

            return swdbDAO.findByQuery("DataEntry", "application = '{0}'".format(crudContext.currentApplicationName), { pagesize: 10, pagenumber: internalListContext.pageNumber }).then(function (results) {
                internalListContext.lastPageLoaded = internalListContext.lastPageLoaded + 1;
                for (var i = 0; i < results.length; i++) {
                    crudContext.itemlist.push(results[i].datamap);
                }
                return $q.when(results);
            });
        },


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

        loadDetail: function (item) {
            if (!crudContext.currentDetailSchema) {
                crudContext.currentDetailSchema = loadDetailSchema();
            }
            crudContext.currentDetailItem = item;
            setPreviousAndNextItems(item);
            contextService.insertIntoContext("crudcontext", crudContext);
            routeService.go("main.cruddetail.maininput");
        },

        loadApplicationGrid: function (applicationName, applicationTitle, schemaId) {
            crudContext.currentTitle = applicationTitle;
            var application = metadataModelService.getApplicationByName(applicationName);

            crudContext.currentApplicationName = applicationName;
            crudContext.currentApplication = application;
            crudContext.currentListSchema = offlineSchemaService.locateSchema(application, schemaId);


            crudContext.currentDetailSchema = loadDetailSchema();


            swdbDAO.findByQuery("DataEntry", "application = '{0}'".format(applicationName), { pagesize: 10, pagenumber: 1 })
                .success(function (results) {
                    internalListContext.lastPageLoaded = 1;
                    crudContext.itemlist = [];
                    for (var i = 0; i < results.length; i++) {
                        results[i].datamap[constants.localIdKey] = results[i].id;
                        crudContext.itemlist.push(results[i].datamap);
                    }
                    routeService.go("main.crudlist");
                    contextService.insertIntoContext("crudcontext", crudContext);
                });
        }
    }

});