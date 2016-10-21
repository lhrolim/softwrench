(function (angular, persistence, _) {
    "use strict";

    try {
        angular.module("persistence.offline");
    } catch (err) {
        return;
    }

    //#region audit.offline module
    const audit = angular.module("audit.offline", ["persistence.offline"]);
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

        function offlineAuditService($q, entities, dao, securityService, routeService) {
            //#region Utils
            function validateEntryField(dict, field) {
                if (!dict[field]) {
                    throw new Error(`IllegalArgumentError: AuditEntry cannot have an empty '${field}'`);
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
                return dao.instantiate("AuditEntry", dict);
            }

            //#endregion

            //#region Public methods

            /**
             * Registers an AuditEntry.
             * The entry should follow the format:
             * {
             *  operation: String, // name of the operation, such as crud_create, crud_update, or a custom one
             *  originaldatamap: Object, // a datamap before an action has happened on the entry
             *  datamap: Object, // datamap after the action has been performed, both will be used to generate a diff
             *  refApplication: String, // name of the application/entity being affected
             *  refClientId: String, // local id of the entity being affected
             *  refId: String, // server's id of the entity being affected
             *  createdBy: String, // username of the user who triggered the operation -> defaults to current logged user if omitted
             * }
             * The fields "operation", "refApplication" and "refClientId" are mandatory.
             * The other fields can be ommited depending on the action being executed, this method is intended for registering
             * complete entries though. See other methods (such as {@link #registerEvent}) for registering customized entries.
             * 
             * @param {Object} entry dictionary in the aforementioned format
             * @returns {Promise<entities.AuditEntry>} resolved with the registered AuditEntry
             * @throws {Error} if any of the mandatory fields is ommited and/or there's no user logged in
             */
            function registerEntry(entry) {
                return instantiateEntry(entry).then(auditentry => dao.save(auditentry));
            }

            /**
             * Register an "Audit Event" type entry. 
             * This entry has no data change (create, update, delete) associated with it
             * i.e. it has no tracking of other entities's datamaps.
             * Usage example (user scanned an asset):
             * offlineAuditService.registerEvent("scan", "asset", asset.id, asset.remoteId, auth.currentUser());
             * 
             * @param {String} operation name of the operation/event being tracked
             * @param {String} refApplication name of the application/entity being affected by the event
             * @param {String} refClientId local id of the enity being affected by the event
             * @param {String} refId server's id of the entity being affected by the event
             * @param {String} refUserId server's userId of the entity being affected by the event
             * @param {String} createdBy username of the user who triggered the event -> defaults to current logged user if omitted  
             * @return {Promise<entities.AuditEntry>} resolved with the registered AuditEntry
             * @throws {Error} if any of the parameters is omitted and/or there's no user logged in
             */
            function registerEvent(operation, refApplication, refClientId, refId, refUserId, createdBy) {
                const entry = {
                    operation,
                    refApplication,
                    refClientId,
                    refId,
                    refUserId,
                    createdBy
                };
                return registerEntry(entry);
            }

            /**
             * Lists all apllications that have AuditEntries related to them (refApplication).
             * 
             * @returns {Promise<Array<String>>} resolved with array of name of the applications.
             */
            function listAudittedApplications() {
                const createdBy = securityService.currentUser();
                return dao.executeStatement(entities.AuditEntry.listApplicationsStatement, [createdBy])
                    .then(results => _.pluck(results, "refApplication"));
            }

            /**
             * Fetches AuditEntries from the Database matching the values passed as arguments.
             * The list can be optionally paginated.
             * 
             * @param {String} refApplication 
             * @param {Object?} paginationOptions dicitionary: 
             *              { 
             *              "pagenumber": Integer, // page to fetch 
             *              "pagesize": Integer // number of items per page
             *              }
             *           won't paginate if parameter is omitted
             * @returns {Promise<Array<entities.AuditEntry>>} resolved with AuditEntry list 
             */
            function listEntries(refApplication, paginationOptions) {
                const createdBy = securityService.currentUser();
                const query = entities.AuditEntry.listPattern.format(refApplication, createdBy);
                const options = { orderby: "createdDate", orderbyascendig: false };
                if (!!paginationOptions) {
                    options.pagesize = paginationOptions["pagesize"];
                    options.pageNumber = paginationOptions["pagenumber"];
                }
                return dao.findByQuery("AuditEntry", query, options);
            }

            /**
             * Fetches the AuditEntry that has matching id.
             * 
             * @param {String} id primary key in the database 
             * @returns {Promise<entities.AuditEntry>} resolved with the AuditEntry
             */
            function getAuditEntry(id) {
                return dao.findById("AuditEntry", id);
            }

            /**
             * Fetches the DataEntry the entry is tracking.
             * 
             * @param {AuditEntry} entry 
             * @returns {Promise<entities.DataEntry>} resolved with the entity 
             */
            function getTrackedEntity(entry) {
                return dao.findById("DataEntry", entry.refClientId);
            }

            /**
             * @param {String} DataEntry's id
             * @param {String} application DataEntry's application
             * @returns {Promise<Array<entites.DataEntry>>} resolved with list of AuditEntries related to the entity
             */
            function getEntriesForEntity(entityId, application) {
                return dao.findByQuery("AuditEntry", entities.AuditEntry.forEntityPattern.format(entityId, application));
            }

            /**
             * Deletes all AuditEntries tracking the entity.
             * 
             * @param {String} entityId DataEntry's id
             * @param {String} 
             * @returns {Promise<Void>} 
             */
            function deleteRelatedEntries(entityId, application) {
                return dao.executeStatement(entities.AuditEntry.deleteRelatedStatement, [entityId, application]);
            }

            /**
             * Navigates to audit's entry-point state (application select screen).
             * 
             * @returns {Promise<Void>} 
             */
            function goToAudit() {
                return routeService.go("main.audit.applicationselect");
            }

            //#endregion

            //#region Service Instance
            const service = {
                registerEvent,
                registerEntry,
                listEntries,
                listAudittedApplications,
                getAuditEntry,
                getTrackedEntity,
                getEntriesForEntity,
                deleteRelatedEntries,
                goToAudit
            };
            return service;
            //#endregion
        };

        //#region Service registration
        audit.factory("offlineAuditService", ["$q", "offlineEntities", "swdbDAO", "securityService", "routeService", offlineAuditService]);
        //#endregion
    })(audit);
    //#endregion

})(angular, persistence, _);