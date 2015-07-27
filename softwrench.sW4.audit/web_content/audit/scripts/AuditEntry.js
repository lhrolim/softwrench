
(function (angular, persistence, entities) {
    "use strict";

    angular.module("audit.offline").factory("offlineAuditService", [offlineAuditService]);

    function offlineAuditService() {

        //#region Utils

        function defineEntities() {
            if (!persistence) {
                //only if persistence storage is defined
                return;
            }

            entities = entities || {};

            ///
            /// OffLine auditEntry
            ///
            entities.AuditEntry = persistence.define('AuditEntry', {
                //the name of the operation, such as crud_create,crud_update, or a custom one
                operation: 'TEXT',
                //this is a datamap before an action has happened on the entry
                originaldatamap: 'JSON',
                //this is the datamap after the action has been performed, both will be used to generate a diff
                datamap: 'JSON',

                refApplication: 'TEXT',
                refId: 'TEXT',
                createdBy: 'TEXT',
                createdDate: 'DATE',
            });
        }

        //#endregion

        //#region Public methods

        function registerEntry(operation,refApplication,refId) {

        }

        //#endregion

        //#region Service Instance

        var service = {
            registerEntry: registerEntry
        };

        defineEntities();

        return service;

        //#endregion
    }


})(angular,persistence, entities);