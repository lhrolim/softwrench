(function (angular, persistence) {
    "use strict";

    //#region audit.offline module
    var audit = angular.module("audit.offline", ["persistence.offline"]);
    //#endregion

    //#region audit.offline entities
    angular.module("persistence.offline").config(["offlineEntitiesProvider", function (offlineEntitiesProvider) {
        var entities = offlineEntitiesProvider.entities;

        entities.AuditEntry = persistence.define("AuditEntry", {
            //the name of the operation, such as crud_create,crud_update, or a custom one
            operation: "TEXT",
            //this is a datamap before an action has happened on the entry
            originaldatamap: "JSON",
            //this is the datamap after the action has been performed, both will be used to generate a diff
            datamap: "JSON",

            refApplication: "TEXT",
            refId: "TEXT",
            createdBy: "TEXT",
            createdDate: "DATE"
        });
    }]);
    //#endregion

    //#region offlineAuditService
    (function(audit) {

        function offlineAuditService($q, swdbDAO) {
            //#region Utils
            function validateEntryField(dict, field) {
                if (!dict[field]) {
                    throw new Error("IllegalArgumentError: AuditEntry cannot have an empty '" + field + "'");
                }
            }

            function instantiateEntry(dict) {
                validateEntryField(dict, "operation");
                validateEntryField(dict, "refApplication");
                validateEntryField(dict, "createdBy");

                var entry = new entities.AuditEntry();
                for (key in dict) {
                    if (!dict.hasOwnProperty(key)) {
                        continue;
                    }
                    entry[key] = dict[key];
                }
                return entry;
            }

            function saveEntry(auditEntry) {
                var deferred = $q.defer();
                persistence.add(auditEntry);
                persistence.flush(function () {
                    deferred.resolve(auditEntry);
                });
                return deferred.promise;
            }

            //#endregion

            //#region Public methods

            /**
             * Registers an AuditEntry.
             * The entry should follow the format:
             * {
             *  operation: String, // name of the operation, such as crud_create, crud_update, or a custom one
             *  originaldatamap: {}, // a datamap before an action has happened on the entry
             *  datamap: {}, // datamap after the action has been performed, both will be used to generate a diff
             *  refApplication: String, // name of the application/entity being affected
             *  refId: String, // id of the entity being affected
             *  createdBy: String, // username of the user who triggered the operation
             * }
             * Fields can be ommited depending on the action being executed, this method is intended for registering
             * complete entries though. See other methods (such as registerEvent) for registering customized entries.
             * 
             * @param {} entry dictionary in the aforementioned format
             * @returns Promise resolved with the registered AuditEntry
             */
            function registerEntry(entry) {
                var auditEntry = instantiateEntry(entry);
                return saveEntry(auditEntry);
            }

            /**
             * Register an "Audit Event" type entry. 
             * This entry has no data change (create, update, delete) associated with it
             * i.e. it has no tracking of other entities's datamaps.
             * 
             * @param String operation name of the operation/event being tracked
             * @param String refApplication name of the application/entity being affected by the event
             * @param String refId id of the enity being affected by the event
             * @param String createdBy username of the user who triggered the event
             * @return Promise resolved with the registered AuditEntry
             */
            function registerEvent(operation, refApplication, refId, createdBy) {
                var entry = {
                    operation: operation,
                    refApplication: refApplication,
                    refId: refId,
                    createdBy: createdBy,
                    createdDate: new Date()
                };
                return registerEntry(entry);
            }

            /**
             * Fetches AuditEntries from the Database matching the values passed as arguments.
             * The list can be optionally paginated.
             * 
             * @param String operation 
             * @param String refApplication 
             * @param String createdBy 
             * @param {} paginationOptions dicitionary: 
             *              { 
             *              "pagenumber": Integer, // page to fetch 
             *              "pagesize": Integer // number of items per page
             *              }
             * @returns Promise resolved with AuditEntry list 
             */
            function listEntries(operation, refApplication, createdBy, paginationOptions) {
                // base query
                var filter = entities.AuditEntry.all()
                    .filter("operation", "=", operation)
                    .filter("refApplication", "=", refApplication)
                    .filter("createdBy", "=", createdBy);

                if (!!paginationOptions) {
                    // apply pagination options
                    var pagesize = paginationOptions["pagesize"];
                    var pagenumber = paginationOptions["pagenumber"];
                    filter = filter.limit(pagesize).skip((pagesize * (pagenumber - 1)));
                }

                var deferred = $q.defer();
                filter.list(function (result) {
                    deferred.resolve(result);
                });
                // resolved with list
                return deferred.promise;
            }

            //#endregion

            //#region Service Instance
            var service = {
                registerEvent: registerEvent,
                registerEntry: registerEntry,
                listEntries: listEntries
            };

            return service;
            //#endregion
        };

        //#region Service registration
        audit.factory("offlineAuditService", ["$q", "swdbDAO", offlineAuditService]);
        //#endregion
    })(audit);
    //#endregion

})(angular, persistence);