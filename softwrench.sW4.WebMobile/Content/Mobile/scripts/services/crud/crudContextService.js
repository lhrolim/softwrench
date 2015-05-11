mobileServices.factory('crudContextService', function ($q, $state,swdbDAO, metadataModelService, offlineSchemaService) {

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

        itemlist: function () {
            return crudContext.itemlist;
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

        loadApplicationGrid: function (applicationName, applicationTitle, schemaId) {
            crudContext.currentTitle = applicationTitle;
            var application = metadataModelService.getApplicationByName(applicationName);

            crudContext.currentApplicationName = applicationName;
            crudContext.currentApplication = application;
            crudContext.currentListSchema = offlineSchemaService.locateSchema(application, schemaId);
            

            swdbDAO.findByQuery("DataEntry", "application = '{0}'".format(applicationName), { pagesize: 10, pagenumber: 1 })
                .success(function (results) {
                    internalListContext.lastPageLoaded = 1;
                    crudContext.itemlist = [];
                    for (var i = 0; i < results.length; i++) {
                        crudContext.itemlist.push(results[i].datamap);
                    }
                });
            $state.go("main.crudlist");
        }
    }

});