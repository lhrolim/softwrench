mobileServices.factory('crudContextService', function ($q, $state,swdbDAO) {

    var crudContext= {
        currentApplication: null,
        currentApplicationTitle: null,
        currentSchema:null,
        previousItemId:null,
        nextItemId: null,
        itemlist:null,
    }

    return {
        
        loadApplication: function (applicationName,applicationTitle, schema) {
            crudContext.currentApplication = applicationName;
            crudContext.currentSchema = schema;
            crudContext.currentApplicationTitle = applicationTitle;
            swdbDAO.findAll("DataEntry", { pagesize: 10, pagenumber: 1 }).success(function(results) {
                crudContext.itemlist = results;
            });
            $state.go("main.crudlist");
        }
    }

});