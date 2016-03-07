(function (angular) {
    'use strict';

    angular.module("sw_mobile_services").factory("offlineSchemaService", ["$log", "fieldService", "schemaService", "securityService", "dispatcherService", offlineSchemaService]);

    function offlineSchemaService($log, fieldService, schemaService, securityService, dispatcherService) {

        var service = {
            loadDetailSchema: loadDetailSchema,
            locateSchema: locateSchema,
            locateSchemaByStereotype: locateSchemaByStereotype,
            fillDefaultValues: fillDefaultValues,
            locateAttributeByQualifier: locateAttributeByQualifier
        };

        return service;

        function loadDetailSchema(currentListSchema, currentApplication, selectedItem) {
            var detailSchemaId = "detail";
            var schemaDetailService = schemaService.getProperty(currentListSchema, "list.click.service");
            var overridenSchema =  !!schemaDetailService && !!selectedItem
                                    ? dispatcherService.invokeServiceByString(schemaDetailService, [currentApplication, currentListSchema, selectedItem]) 
                                    : schemaService.getProperty(currentListSchema, "list.click.schema");
            if (overridenSchema) {
                detailSchemaId = overridenSchema;
            }
            return this.locateSchema(currentApplication, detailSchemaId);
        };

        function locateSchema(application, schemaId) {
            var schemasList = application.data.schemasList;

            for (var i = 0; i < schemasList.length; i++) {
                var schema = schemasList[i];
                if ("list".equalsIc(schema.stereotype)) {
                    //build dict cache
                    buildQualifierCache(schema);
                }
                if (schema.schemaId.equalsIc(schemaId)) {
                    return schema;
                }
            }
            return null;
        };


        function buildQualifierCache(schema) {
            schema.jscache = schema.jscache || {};
            if (schema.jscache.griddisplayables) {
                //already cached
                return;
            }
            schema.jscache.qualifiercache = {};
            var displayables = schema.displayables;
            for (var i = 0; i < displayables.length; i++) {
                var displayable = displayables[i];
                if (displayable.qualifier) {
                    schema.jscache.qualifiercache[displayable.qualifier] = displayable;
                }
            }
        };

        function locateSchemaByStereotype(application, stereotype) {
            var schemasList = application.data.schemasList;

            for (var i = 0; i < schemasList.length; i++) {
                var schema = schemasList[i];
                if (schema.stereotype == "list") {
                    //build dict cache
                    buildQualifierCache(schema);
                }
                if (stereotype.equalsIc(schema.stereotype)) {
                    return schema;
                }
                else if ("detail".equalsIc(stereotype) && schema.schemaId.equalsIc("detail")) {
                    return schema;
                }
            }
            return null;
        };

        function setValueIfNotPresent(item, field, value) {
            if (!item[field]) {
                item[field] = value;
            }
        }

        function fillDefaultOfflineValues(schema, item) {
            var user = securityService.currentFullUser();
            // TODO: handle case where no default value is supposed to be used (client forcefully regitered empty default value)
            setValueIfNotPresent(item, "siteid", user["SiteId"]);
            setValueIfNotPresent(item, "orgid", user["OrgId"]);
        };

        function fillDefaultValues(schema, item, parent) {
            var scope = !!parent ? { previousdata: { fields: parent }, parentdata: { fields: parent } } : {};
            fieldService.fillDefaultValues(schema.displayables, item, scope);
            fillDefaultOfflineValues(schema, item);
        };

        function locateAttributeByQualifier(schema, qualifier) {
            schema.jscache = schema.jscache || {};
            if (schema.jscache.qualifiercache && schema.jscache.qualifiercache[qualifier]) {
                //already cached
                return schema.jscache.qualifiercache[qualifier].attribute;
            }
            buildQualifierCache(schema);
            return schema.jscache.qualifiercache[qualifier].attribute;
        }

    }
})(angular);