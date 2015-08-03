(function (offlineMaximoApplications) {
    "use strict";

    function maximoDataService(swdbDAO, $q, $filter) {

        //#region Utils

        var capitalizer = $filter("capitalize");

        //#endregion

        //#region Public methods

        /**
         * Searches for the DataEntry with matching application and refId 
         * matching the entity's 'userId' which is determined by the schema's 'userIdFieldName'.
         * 
         * @param String application 
         * @param {} schema 
         * @param String refId 
         * @returns Promise resolved with the entry found, rejected with error if no entry is found.  
         */
        function loadItemByMaximoUid(application, schema, refId) {
            var userId = schema.userIdFieldName;
            return swdbDAO.findSingleByQuery("DataEntry", 'application = \'{0}\' and datamap like \'%"{1}":"{2}"%\''.format(application, userId, refId))
                .then(function (item) {
                    if (!item) {
                        var applicationName = capitalizer(application);
                        return $q.reject("{0} {1} not found".format(applicationName, refId));
                    }
                    return item;
                });
        }

        //#endregion

        //#region Service Instance

        var service = {
            loadItemByMaximoUid: loadItemByMaximoUid
        };

        return service;

        //#endregion
    }

    //#region Service registration
    offlineMaximoApplications.factory("maximoDataService", ["swdbDAO", "$q", "$filter", maximoDataService]);
    //#endregion

})(offlineMaximoApplications);
