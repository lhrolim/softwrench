(function (angular, persistence) {
    "use strict";

    try {
        angular.module("persistence.offline");
    } catch (err) {
        return;
    }

    //#region audit.offline module
    var audit = angular.module("audit.offline", ["persistence.offline"]);
    //#endregion

    //#region audit.offline migrations
    angular.module("persistence.offline").config(["offlineMigrationsProvider", function (offlineMigrationsProvider) {
        const migrationGroup = offlineMigrationsProvider.createMigrationGroup(10, "offline audit migrations");

        migrationGroup.addMigration("table AuditEntry", {
            up: function () {
                this.createTable("AuditEntry", (t) => {
                    t.text("operation");
                    t.json("originaldatamap");
                    t.json("datamap");
                    t.text("refApplication");
                    t.text("refClientId");
                    t.text("refId");
                    t.text("refUserId");
                    t.text("createdBy");
                    t.date("createdDate");
                });
            },
            down: function () {
                this.dropTable("AuditEntry");
            }
        });
    }]);
    //#endregion

    //#region audit.offline entities
    angular.module("persistence.offline").config(["offlineEntitiesProvider", function (offlineEntitiesProvider) {
        const entities = offlineEntitiesProvider.entities;

        entities.AuditEntry = persistence.define("AuditEntry", {
            //the name of the operation, such as crud_create, crud_update, or a custom one
            operation: "TEXT",
            //this is a datamap before an action has happened on the entry
            originaldatamap: "JSON",
            //this is the datamap after the action has been performed, both will be used to generate a diff
            datamap: "JSON",
            // application/entity being tracked
            refApplication: "TEXT",
            // local/persistence's id of the entity being tracked
            refClientId: "TEXT",
            // server's/maximo's id of the entity being tracked
            refId: "TEXT",
            // server's/maximo's userId of the entity being tracked (such as an asset's assetnum)
            refUserId: "TEXT",

            createdBy: "TEXT",
            createdDate: "DATE"
        });

        entities.AuditEntry.listPattern = "refApplication='{0}' and createdBy='{1}'";
        entities.AuditEntry.listApplicationsStatement = "select distinct refApplication from AuditEntry where createdBy=?";
        entities.AuditEntry.forEntityPattern = "refClientId='{0}' and refApplication='{1}'";
        entities.AuditEntry.deleteRelatedStatement = "delete from AuditEntry where refClientId=? and refApplication=?";
        //TODO: treat the case where AuditEntries that have no refId shouldn't be deleted (e.g. crud_create operations)
        entities.AuditEntry.deleteRelatedByRefIdStatement = "delete from AuditEntry where refApplication=? and (refId in ({0}) or refId is null)";
    }]);
    //#endregion

    //#region offlineAuditService
    (function (audit) {

        function offlineAuditService($q, entities, swdbDAO, securityService) {
            //#region Utils
            function validateEntryField(dict, field) {
                if (!dict[field]) {
                    throw new Error("IllegalArgumentError: AuditEntry cannot have an empty '" + field + "'");
                }
            }

            function instantiateEntry(dict) {
                if (!dict["createdBy"]) {
                    dict["createdBy"] = securityService.currentUser();
                }
                validateEntryField(dict, "operation");
                validateEntryField(dict, "refApplication");
                validateEntryField(dict, "refClientId");
                validateEntryField(dict, "createdBy");

                dict["createdDate"] = new Date();
                return swdbDAO.instantiate("AuditEntry", dict);
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
             *  refClientId: String, // local id of the entity being affected
             *  refId: String, // server's id of the entity being affected
             *  createdBy: String, // username of the user who triggered the operation -> defaults to current logged user if omitted
             * }
             * The fields "operation", "refApplication" and "refClientId" are mandatory.
             * The other fields can be ommited depending on the action being executed, this method is intended for registering
             * complete entries though. See other methods (such as {@link #registerEvent}) for registering customized entries.
             * 
             * @param {} entry dictionary in the aforementioned format
             * @returns Promise resolved with the registered AuditEntry
             * @throws Error if any of the mandatory fields is ommited and/or there's no user logged in
             */
            function registerEntry(entry) {
                return instantiateEntry(entry).then(function (auditentry) {
                    return swdbDAO.save(auditentry);
                });

            }

            /**
             * Register an "Audit Event" type entry. 
             * This entry has no data change (create, update, delete) associated with it
             * i.e. it has no tracking of other entities's datamaps.
             * Usage example (user scanned an asset):
             * offlineAuditService.registerEvent("scan", "asset", asset.id, asset.remoteId, auth.currentUser());
             * 
             * @param String operation name of the operation/event being tracked
             * @param String refApplication name of the application/entity being affected by the event
             * @param String refClientId local id of the enity being affected by the event
             * @param String refId server's id of the entity being affected by the event
             * @param String refUserId server's userId of the entity being affected by the event
             * @param String createdBy username of the user who triggered the event -> defaults to current logged user if omitted  
             * @return Promise resolved with the registered AuditEntry
             * @throws Error if any of the parameters is omitted and/or there's no user logged in
             */
            function registerEvent(operation, refApplication, refClientId, refId, refUserId, createdBy) {
                var entry = {
                    operation: operation,
                    refApplication: refApplication,
                    refClientId: refClientId,
                    refId: refId,
                    refUserId: refUserId,
                    createdBy: createdBy
                };
                return registerEntry(entry);
            }

            /**
             * Lists all apllications that have AuditEntries related to them (refApplication).
             * 
             * @returns Promise resolved with array of name of the applications.
             */
            function listAudittedApplications() {
                var createdBy = securityService.currentUser();
                return swdbDAO.executeStatement(entities.AuditEntry.listApplicationsStatement, [createdBy])
                    .then(function (results) {
                        return results.map(function (r) {
                            return r.refApplication;
                        });
                    });
            }

            /**
             * Fetches AuditEntries from the Database matching the values passed as arguments.
             * The list can be optionally paginated.
             * 
             * @param String refApplication 
             * @param {} paginationOptions dicitionary: 
             *              { 
             *              "pagenumber": Integer, // page to fetch 
             *              "pagesize": Integer // number of items per page
             *              }
             *           won't paginate if parameter is omitted
             * @returns Promise resolved with AuditEntry list 
             */
            function listEntries(refApplication, paginationOptions) {
                var createdBy = securityService.currentUser();
                var query = entities.AuditEntry.listPattern.format(refApplication, createdBy);
                var options = { orderby: "createdDate", orderbyascendig: false };
                if (!!paginationOptions) {
                    options.pagesize = paginationOptions["pagesize"];
                    options.pageNumber = paginationOptions["pagenumber"];
                }
                return swdbDAO.findByQuery("AuditEntry", query, options);
            }

            /**
             * Fetches the AuditEntry that has matching id.
             * 
             * @param String id primary key in the database 
             * @returns Promise resolved with the AuditEntry
             */
            function getAuditEntry(id) {
                return swdbDAO.findById("AuditEntry", id);
            }

            /**
             * Fetches the DataEntry the entry is tracking.
             * 
             * @param AuditEntry entry 
             * @returns Promise resolved with the entity 
             */
            function getTrackedEntity(entry) {
                return swdbDAO.findById("DataEntry", entry.refClientId);
            }

            /**
             * @param String DataEntry's id
             * @param String application DataEntry's application
             * @returns Promise resolved with list of AuditEntries related to the entity
             */
            function getEntriesForEntity(entityId, application) {
                return swdbDAO.findByQuery("AuditEntry", entities.AuditEntry.forEntityPattern.format(entityId, application));
            }

            /**
             * Deletes all AuditEntries tracking the entity.
             * 
             * @param String entityId DataEntry's id
             * @param String 
             * @returns Promise 
             */
            function deleteRelatedEntries(entityId, application) {
                return swdbDAO.executeStatement(entities.AuditEntry.deleteRelatedStatement, [entityId, application]);
            }

            //#endregion

            //#region Service Instance
            var service = {
                registerEvent: registerEvent,
                registerEntry: registerEntry,
                listEntries: listEntries,
                listAudittedApplications: listAudittedApplications,
                getAuditEntry: getAuditEntry,
                getTrackedEntity: getTrackedEntity,
                getEntriesForEntity: getEntriesForEntity,
                deleteRelatedEntries: deleteRelatedEntries
            };

            return service;
            //#endregion
        };

        //#region Service registration
        audit.factory("offlineAuditService", ["$q", "offlineEntities", "swdbDAO", "securityService", offlineAuditService]);
        //#endregion
    })(audit);
    //#endregion

})(angular, persistence);