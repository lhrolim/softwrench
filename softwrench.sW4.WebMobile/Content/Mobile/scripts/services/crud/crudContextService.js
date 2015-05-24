mobileServices.factory('crudContextService', function ($q, $state, swdbDAO, metadataModelService, offlineSchemaService, schemaService, contextService, routeService) {

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
        previousItemId: null,
        nextItemId: null,

    }

    if (isRippleEmulator()) {
        var savedCrudContext = contextService.getFromContext("crudcontext");
        if (savedCrudContext) {
            crudContext = JSON.parse(savedCrudContext);
        }
    }


    function loadDetailSchema() {
        var detailSchemaId = "detail";
        var overridenSchema = schemaService.getProperty(crudContext.currentListSchema, "list.click.schema");
        if (overridenSchema) {
            detailSchemaId = overridenSchema;
        }
        return offlineSchemaService.locateSchema(crudContext.currentApplication, detailSchemaId);
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

        mainDisplayables:function() {
            return schemaService.nonTabFields(crudContext.currentDetailSchema);
        },

        loadMorePromise: function () {

            return swdbDAO.findByQuery("DataEntry", "application = '{0}'".format(crudContext.currentApplicationName), { pagesize: 10, pagenumber: internalListContext.pageNumber }).then(function (results) {
                internalListContext.lastPageLoaded = internalListContext.lastPageLoaded + 1;
                for (var i = 0; i < results.length; i++) {
                    crudContext.itemlist.push(results[i].datamap);
                }
                return $q.when(results);
            });
        },

        loadDetail: function (item) {
            if (!crudContext.currentDetailSchema) {
                crudContext.currentDetailSchema = loadDetailSchema();
            }
            crudContext.currentDetailItem = item;
            routeService.go("main.cruddetail");
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
                        crudContext.itemlist.push(results[i].datamap);
                    }
                    routeService.go("main.crudlist");
                    contextService.insertIntoContext("crudcontext", crudContext);
                });
        }
    }

});