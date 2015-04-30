mobileServices.factory('crudContextService', function ($q, $state, swdbDAO) {

    var crudContext = {
        currentApplication: null,
        currentTitle: null,
        currentSchema: null,
        previousItemId: null,
        nextItemId: null,
        itemlist: null,
    }

    return {

        currentTitle:function() {
            return crudContext.currentTitle;
        },

        loadApplication: function (applicationName, applicationTitle, schema) {
            crudContext.currentApplication = applicationName;
            crudContext.currentSchema = schema;
            crudContext.currentTitle = applicationTitle;
            swdbDAO.findByQuery("DataEntry", "application = {0}".format(applicationName), { pagesize: 10, pagenumber: 1 })
                .success(function (results) {
                crudContext.itemlist = results;
            });
            $state.go("main.crudlist");
        }
    }

});