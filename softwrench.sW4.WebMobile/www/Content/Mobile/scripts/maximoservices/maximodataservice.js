(function (offlineMaximoApplications) {
    "use strict";

    function maximoDataService(dao, $q, $filter) {

        //#region Utils

        const capitalizer = $filter("capitalize");

        function loadEntries(application, field, value, list) {
            const method = list ? "findByQuery" : "findSingleByQuery";

            return dao[method]("DataEntry", `application='${application}' and datamap like '%"${field}":"${value}"%'`)
                .then(items => {
                    if (!items || (list && items.length <= 0)) {
                        return $q.reject(new Error(`${capitalizer(application)} with ${field} = ${value} not found`));
                    }
                    return items;
                });
        }

        //#endregion

        //#region Public methods

        /**
         * Finds the DataEntries matching the application and with field=value in their datamap.
         * 
         * @param {String} application 
         * @param {String} field
         * @param {Any} value 
         * @returns {Promise<Array<entities.DataEntry>>} resolved with the entry found, rejected with error if no entry is found. 
         */
        function loadItemsByField(application, field, value) {
            return loadEntries(application, field, value, true);
        }

        /**
         * Finds the DataEntry matching the application and with field=value in it's datamap.
         * 
         * @param {String} application 
         * @param {String} field
         * @param {Any} value 
         * @returns {Promise<entities.DataEntry>} resolved with the entry found, rejected with error if no entry is found. 
         */
        function loadSingleItemByField(application, field, value) {
            return loadEntries(application, field, value);
        }

        /**
         * Searches for the DataEntry with matching application and refId 
         * matching the entity's 'userId' which is determined by the schema's 'userIdFieldName'.
         * 
         * @param {String} application 
         * @param {Schema}
         * @param {String} refId 
         * @returns {Promise<entities.DataEntry>} resolved with the entry found, rejected with error if no entry is found.  
         */
        function loadItemByMaximoUid(application, schema, refId) {
            return loadSingleItemByField(application, schema.userIdFieldName, refId);
        }

        //#endregion

        //#region Service Instance
        const service = {
            loadItemsByField,
            loadSingleItemByField,
            loadItemByMaximoUid
        };
        return service;

        //#endregion
    }

    //#region Service registration
    offlineMaximoApplications.factory("maximoDataService", ["swdbDAO", "$q", "$filter", maximoDataService]);
    //#endregion

})(offlineMaximoApplications);
